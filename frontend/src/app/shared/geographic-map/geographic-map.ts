import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  inject,
  input,
  output,
} from '@angular/core';
// The default import is Leaflet's own object. leaflet-routing-machine adds `L.Routing` to that
// object when it loads; a `* as L` import is a copy made earlier and would never see it.
import L from 'leaflet';
import 'leaflet-routing-machine';

export interface MapCoordinate {
  latitude: number;
  longitude: number;
}

// A pin with an id reports that id when clicked; a pin without one behaves like the map
// under it.
export interface MapPin extends MapCoordinate {
  id?: string;
}

// A circle drawn around a center, with the radius in meters.
export interface MapCircle {
  center: MapCoordinate;
  radiusMeters: number;
}

@Component({
  selector: 'app-geographic-map',
  templateUrl: './geographic-map.html',
  styleUrl: './geographic-map.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    role: 'application',
    'aria-label': 'Geografska mapa',
  },
})
export class GeographicMap implements AfterViewInit, OnChanges, OnDestroy {
  readonly pins = input<readonly MapPin[]>([]);
  readonly path = input<readonly MapCoordinate[] | null>(null);
  readonly selectionEnabled = input(false);
  // With false, a selection click is still reported, but the map draws no marker of its own;
  // a page that shows its points as pins uses it, so a removed point leaves nothing behind.
  readonly showSelectedLocation = input(true);
  // Simulator mode: a click reports where the explorer is. At most one click mode works at a
  // time, so while the simulator is on, a click never selects a location.
  readonly simulatorEnabled = input(false);
  // The explorer's saved position, drawn as the "ja" marker; null draws nothing.
  readonly explorerPosition = input<MapCoordinate | null>(null);
  // At most one circle, drawn exactly as given; null draws nothing.
  readonly circle = input<MapCircle | null>(null);
  // Points of a walking route along the streets, in order; null draws nothing.
  readonly route = input<readonly MapCoordinate[] | null>(null);

  readonly pathLengthKm = output<number | null>();
  readonly routeLengthKm = output<number | null>();
  readonly routeFailed = output<void>();
  readonly locationSelected = output<MapCoordinate>();
  readonly explorerMoved = output<MapCoordinate>();
  readonly pinSelected = output<string>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private map: L.Map | null = null;
  private pinLayer: L.LayerGroup | null = null;
  private pathLayer: L.LayerGroup | null = null;
  private selectedMarker: L.Marker | null = null;
  private explorerMarker: L.Marker | null = null;
  private circleShape: L.Circle | null = null;
  private routeLayer: L.LayerGroup | null = null;
  // The public walking OSRM server of FOSSGIS, meant for development and moderate use. The
  // cast is there because the types package marks options as required that are never used here.
  private readonly router: L.Routing.IRouter = L.Routing.osrmv1({
    serviceUrl: 'https://routing.openstreetmap.de/routed-foot/route/v1',
    profile: 'foot',
  } as L.Routing.OSRMOptions);
  // Counts route requests, so an outcome of an older request can be recognised and ignored.
  private routeRequest = 0;

  ngAfterViewInit(): void {
    // One world, not repeated copies side by side: panning stops at the world's edges.
    this.map = L.map(this.host.nativeElement, {
      maxBounds: [
        [-90, -180],
        [90, 180],
      ],
      maxBoundsViscosity: 1,
    }).setView([0, 0], 2);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      noWrap: true,
      maxZoom: 19,
      attribution:
        '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors',
    }).addTo(this.map);

    this.pinLayer = L.layerGroup().addTo(this.map);
    this.pathLayer = L.layerGroup().addTo(this.map);
    this.routeLayer = L.layerGroup().addTo(this.map);
    this.map.on('click', this.handleClick, this);

    this.renderPins();
    this.renderPath(this.path() !== null);
    if (this.route() !== null) {
      this.renderRoute();
    }
    this.renderExplorerPosition();
    this.renderCircle();
    this.fitContent();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (!this.map) {
      return;
    }

    if (changes['pins']) {
      this.renderPins();
    }

    if (changes['path']) {
      this.renderPath(true);
    }

    // Only a new list of points asks for a route; panning and zooming never do.
    if (changes['route']) {
      this.renderRoute();
    }

    if (changes['explorerPosition']) {
      this.renderExplorerPosition();
    }

    if (changes['circle']) {
      this.renderCircle();
    }

    if (changes['showSelectedLocation'] && !this.showSelectedLocation()) {
      this.selectedMarker?.remove();
      this.selectedMarker = null;
    }

    if (changes['pins'] || changes['path'] || changes['route'] || changes['circle']) {
      this.fitContent();
    }
  }

  ngOnDestroy(): void {
    // A route that arrives after the map is gone is ignored.
    this.routeRequest++;
    this.map?.remove();
  }

  private renderPins(): void {
    this.pinLayer?.clearLayers();
    for (const pin of this.pins()) {
      const marker = L.marker([pin.latitude, pin.longitude], {
        icon: this.createIcon('geographic-map__pin-icon'),
      }).addTo(this.pinLayer!);

      // A marker that listens to clicks keeps them to itself, so the map's click handler does
      // not run: no location is selected and the "ja" marker does not move. A pin without an
      // id has no listener, so its click reaches the map as before.
      const id = pin.id;
      if (id !== undefined) {
        marker.on('click', () => this.pinSelected.emit(id));
      }
    }
  }

  private renderPath(emitLength: boolean): void {
    this.pathLayer?.clearLayers();
    const path = this.path();

    if (path === null) {
      if (emitLength) {
        this.pathLengthKm.emit(null);
      }
      return;
    }

    const points = path.map((point) => L.latLng(point.latitude, point.longitude));
    if (points.length > 1) {
      L.polyline(points, { className: 'geographic-map__path' }).addTo(this.pathLayer!);
    }

    if (emitLength) {
      this.pathLengthKm.emit(this.calculateLength(points));
    }
  }

  private renderRoute(): void {
    // A new list makes every earlier request outdated.
    this.routeRequest++;
    const request = this.routeRequest;
    const route = this.route();

    if (route === null) {
      this.routeLayer?.clearLayers();
      this.routeLengthKm.emit(null);
      return;
    }

    if (route.length < 2) {
      // Fewer than two points is not a route: the server is not asked and the length is 0.
      this.routeLayer?.clearLayers();
      this.routeLengthKm.emit(0);
      return;
    }

    const waypoints = route.map((point) =>
      L.Routing.waypoint(L.latLng(point.latitude, point.longitude)),
    );
    // Only the router is used, not the library's control, so there are no markers to drag
    // and no panel with directions. The old line stays until the new outcome arrives.
    this.router.route(waypoints, (error, routes) => {
      if (request !== this.routeRequest) {
        return;
      }

      this.routeLayer?.clearLayers();
      // The first route is the best one; the server may add alternatives after it.
      const found = routes?.[0];
      if (error || !found?.coordinates || !found.summary) {
        this.routeFailed.emit();
        return;
      }

      // Not interactive: the line is only a drawing, so a click on it reaches the map.
      L.polyline(found.coordinates, {
        interactive: false,
        className: 'geographic-map__route',
      }).addTo(this.routeLayer!);
      this.routeLengthKm.emit(found.summary.totalDistance / 1000);
    });
  }

  private renderExplorerPosition(): void {
    const position = this.explorerPosition();

    if (position === null) {
      this.explorerMarker?.remove();
      this.explorerMarker = null;
      return;
    }

    const point = L.latLng(position.latitude, position.longitude);
    if (this.explorerMarker) {
      this.explorerMarker.setLatLng(point);
    } else {
      this.explorerMarker = L.marker(point, {
        icon: this.createIcon('geographic-map__explorer-icon'),
      }).addTo(this.map!);
    }
  }

  private renderCircle(): void {
    const circle = this.circle();

    if (circle === null) {
      this.circleShape?.remove();
      this.circleShape = null;
      return;
    }

    const center = L.latLng(circle.center.latitude, circle.center.longitude);
    if (this.circleShape) {
      this.circleShape.setLatLng(center);
      this.circleShape.setRadius(circle.radiusMeters);
    } else {
      // Not interactive: the circle is only a drawing, so a click inside it reaches the map
      // and the pins inside it, exactly as a click anywhere else.
      this.circleShape = L.circle(center, {
        radius: circle.radiusMeters,
        interactive: false,
        className: 'geographic-map__circle',
      }).addTo(this.map!);
    }
  }

  private calculateLength(points: readonly L.LatLng[]): number {
    let lengthInMeters = 0;
    for (let index = 1; index < points.length; index++) {
      lengthInMeters += points[index - 1].distanceTo(points[index]);
    }
    return lengthInMeters / 1000;
  }

  private fitContent(): void {
    const coordinates = [
      ...this.pins().map((pin) => L.latLng(pin.latitude, pin.longitude)),
      ...(this.path() ?? []).map((point) => L.latLng(point.latitude, point.longitude)),
      // The route's points, not its line: the view fits at once, without a second jump when
      // the line arrives.
      ...(this.route() ?? []).map((point) => L.latLng(point.latitude, point.longitude)),
    ];
    const circle = this.circle();

    if (circle !== null) {
      // The whole circle is visible, together with the pins and the path.
      const bounds = L.latLng(circle.center.latitude, circle.center.longitude).toBounds(
        circle.radiusMeters * 2,
      );
      for (const coordinate of coordinates) {
        bounds.extend(coordinate);
      }
      this.map?.fitBounds(bounds, { padding: [24, 24] });
    } else if (coordinates.length === 1) {
      this.map?.setView(coordinates[0], 13);
    } else if (coordinates.length > 1) {
      this.map?.fitBounds(L.latLngBounds(coordinates), { padding: [24, 24] });
    }
  }

  private readonly handleClick = (event: L.LeafletMouseEvent): void => {
    if (!this.map) {
      return;
    }

    // After the map is panned across the world's edge, Leaflet reports a longitude outside
    // −180..180, so the reported coordinate is wrapped back into that range.
    const wrapped = event.latlng.wrap();
    const coordinate: MapCoordinate = { latitude: wrapped.lat, longitude: wrapped.lng };

    if (this.simulatorEnabled()) {
      // The "ja" marker does not move here; it moves when the page passes the saved position.
      this.explorerMoved.emit(coordinate);
    } else if (this.selectionEnabled()) {
      if (this.showSelectedLocation()) {
        this.selectLocation(event.latlng);
      }
      this.locationSelected.emit(coordinate);
    }
  };

  private selectLocation(point: L.LatLng): void {
    if (this.selectedMarker) {
      this.selectedMarker.setLatLng(point);
    } else {
      this.selectedMarker = L.marker(point, {
        icon: this.createIcon('geographic-map__selected-icon'),
      }).addTo(this.map!);
    }
  }

  private createIcon(className: string): L.DivIcon {
    return L.divIcon({
      className,
      html: '<span aria-hidden="true"></span>',
      iconSize: [22, 22],
      iconAnchor: [11, 11],
    });
  }
}
