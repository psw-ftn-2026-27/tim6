# Deljena geografska mapa

`GeographicMap` je prezentaciona komponenta. Ona ne poziva server i ne zna ništa o
turama ili izazovima. Roditeljska stranica joj prosleđuje podatke i obrađuje događaje.

## Upotreba

Uvezi standalone komponentu i tipove iz `shared/geographic-map/geographic-map`:

```ts
import {
  GeographicMap,
  MapCoordinate,
  MapPin,
} from '../../../shared/geographic-map/geographic-map';

readonly pins: MapPin[] = [
  { latitude: 45.2671, longitude: 19.8335 },
];
readonly path: MapCoordinate[] = [
  { latitude: 45.2671, longitude: 19.8335 },
  { latitude: 45.255, longitude: 19.845 },
];
```

```html
<app-geographic-map
  [pins]="pins"
  [path]="path"
  [selectionEnabled]="true"
  (pathLengthKm)="onPathLength($event)"
  (locationSelected)="onLocationSelected($event)"
/>
```

- `pins` je lista lokacija koje su uvek prikazane.
- `path` je opciona geometrija putanje. Komponenta povezuje tačke zadatim redom i
  emituje dužinu u kilometrima. Kada se putanja ukloni, emituje `null`.
- `selectionEnabled` dozvoljava izbor lokacije klikom. Svaki sledeći klik premešta
  isti izabrani pin i emituje nove koordinate.
- `pathLengthKm` emituje `number | null`, a `locationSelected` emituje
  `MapCoordinate`.

Roditelj je odgovoran za validaciju obavezne lokacije i čuvanje koordinata. Komponenta
ne pronalazi trasu; prikazuje samo geometriju koju dobije.

Mapa koristi standardne OpenStreetMap raster pločice. Atribucija u donjem uglu mora
ostati vidljiva, a javni servis pločica ne sme se koristiti za masovno preuzimanje.
