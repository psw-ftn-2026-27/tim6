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
import * as L from 'leaflet';

export interface MapCoordinate {
  latitude: number;
  longitude: number;
}

export type MapPin = MapCoordinate;

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
  // Simulator mode: a click reports where the explorer is. At most one click mode works at a
  // time, so while the simulator is on, a click never selects a location.
  readonly simulatorEnabled = input(false);
  // The explorer's saved position, drawn as the "ja" marker; null draws nothing.
  readonly explorerPosition = input<MapCoordinate | null>(null);

  readonly pathLengthKm = output<number | null>();
  readonly locationSelected = output<MapCoordinate>();
  readonly explorerMoved = output<MapCoordinate>();

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private map: L.Map | null = null;
  private pinLayer: L.LayerGroup | null = null;
  private pathLayer: L.LayerGroup | null = null;
  private selectedMarker: L.Marker | null = null;
  private explorerMarker: L.Marker | null = null;

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
    this.map.on('click', this.handleClick, this);

    this.renderPins();
    this.renderPath(this.path() !== null);
    this.renderExplorerPosition();
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

    if (changes['explorerPosition']) {
      this.renderExplorerPosition();
    }

    if (changes['pins'] || changes['path']) {
      this.fitContent();
    }
  }

  ngOnDestroy(): void {
    this.map?.remove();
  }

  private renderPins(): void {
    this.pinLayer?.clearLayers();
    for (const pin of this.pins()) {
      L.marker([pin.latitude, pin.longitude], {
        icon: this.createIcon('geographic-map__pin-icon'),
      }).addTo(this.pinLayer!);
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
    ];

    if (coordinates.length === 1) {
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
      this.selectLocation(event.latlng);
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
