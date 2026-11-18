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
- `showSelectedLocation` (podrazumevano `true`) sa vrednošću `false` ne crta izabrani pin,
  a klik i dalje emituje `locationSelected`. Koristi ga stranica koja izabrane tačke sama
  prikazuje kao pinove (npr. ključne tačke u E21), da posle uklanjanja tačke na mapi ne
  ostane izabrani pin.
- `pathLengthKm` emituje `number | null`, a `locationSelected` emituje
  `MapCoordinate`.

Roditelj je odgovoran za validaciju obavezne lokacije i čuvanje koordinata. Za `path`
komponenta ne pronalazi trasu; prikazuje samo geometriju koju dobije. Liniju po ulicama
crta poseban ulaz `route` (odeljak „Ruta po ulicama“).

Mapa prikazuje jedan svet: pločice se ne ponavljaju levo i desno, a mapa ne može da se
pomeri preko ruba sveta. Pre nego što javi klik, mapa uz to svodi geografsku dužinu u
opseg od −180 do 180, kao zaštitu da nijedna emitovana koordinata ne izađe iz opsega.

## Simulator pozicije istraživača

Istraživač koji koristi veb aplikaciju bez telefona klikom na mapu zadaje gde se trenutno
nalazi. Server čuva jednu trenutnu poziciju po istraživaču na `/api/shared/positions/mine`;
zadaje je i čita samo korisnik sa ulogom `explorer`, pa simulator ima smisla samo na
stranicama za istraživače. Na kojim stranicama se uključuje, odlučuje svaki tim.

Mapa je i dalje prezentaciona. Simulator dodaje dva ulaza i jedan izlaz:

- `simulatorEnabled` uključuje režim simulatora. Klik na mapu tada emituje `explorerMoved`
  sa koordinatama klika (`MapCoordinate`), što znači „istraživač je ovde“.
- `explorerPosition` je poslednja sačuvana pozicija (`MapCoordinate | null`). Mapa je
  crta kao okrugli marker „ja“, drugačiji od pinova i od izabrane lokacije. Dok je
  vrednost `null`, marker se ne crta.
- Mapa sama ne pomera marker „ja“ na klik. Marker se pomera tek kad stranica prosledi
  novu poziciju, a to radi tek kad server potvrdi čuvanje.

Na mapi je u svakom trenutku uključen najviše jedan režim klika: izbor lokacije ili
simulator. Stranica može da menja režim dok je otvorena, tako što menja `selectionEnabled`
i `simulatorEnabled`; neka nikad ne uključi oba. Ako su ipak oba uključena, klik ide
simulatoru, a lokacija se ne bira.

### Šta radi stranica

Stranica čita poziciju kao `httpResource`, a čuva je komandom `move` deljenog servisa
`ExplorerPositioning` iz `shared/explorer-positioning/`. Posle uspešne komande ponovo
učita resurs.

```ts
import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { CoordinatesDto } from '../../../../shared/api/coordinates-dto';
import { ExplorerPositioning } from '../../../../shared/explorer-positioning/explorer-positioning';
import { GeographicMap, MapCoordinate } from '../../../../shared/geographic-map/geographic-map';
import { serverMessage } from '../../../../shared/util/server-message';

private readonly explorerPositioning = inject(ExplorerPositioning);

// Server odgovara sa 204 bez tela kad istraživač još nema poziciju; vrednost je tada null.
protected readonly position = httpResource<CoordinatesDto | null>(
  () => '/api/shared/positions/mine',
);
// Dok čitanje traje i kad ne uspe, markera nema.
protected readonly explorerPosition = computed(() =>
  this.position.hasValue() ? this.position.value() : null,
);
protected readonly loadError = computed(() =>
  this.position.error()
    ? serverMessage(this.position.error(), 'Your position could not be loaded.')
    : null,
);
protected readonly moving = signal(false);
protected readonly moveError = signal<string | null>(null);

protected async onExplorerMoved(coordinates: MapCoordinate): Promise<void> {
  // Klik dok čuvanje traje se zanemaruje, da se ne pošalju dva čuvanja odjednom.
  if (this.moving()) {
    return;
  }

  this.moving.set(true);
  this.moveError.set(null);
  try {
    await this.explorerPositioning.move(coordinates);
    this.position.reload();
  } catch (failure) {
    this.moveError.set(serverMessage(failure, 'Your position could not be saved.'));
  } finally {
    this.moving.set(false);
  }
}
```

```html
<app-geographic-map
  [simulatorEnabled]="true"
  [explorerPosition]="explorerPosition()"
  (explorerMoved)="onExplorerMoved($event)"
/>

@if (moveError()) {
  <p class="error">{{ moveError() }}</p>
} @else if (loadError()) {
  <p class="error">{{ loadError() }}</p>
}
```

- „Nema pozicije“ (204, vrednost `null`) nije greška: nema ni poruke ni markera.
- Poruka o grešci stoji ispod mape. Ako server pošalje poruku (`title` u ProblemDetails),
  prikazuje se ona, a inače rezervna poruka stranice. Poruka nestaje pri sledećem kliku,
  jer stranica na početku obrade klika briše grešku.
- Posle neuspešnog čuvanja marker ostaje na poslednjoj sačuvanoj poziciji ili ga i dalje
  nema, jer stranica tada ne učitava resurs ponovo.
- Kad stranici modula treba pozicija (na primer pri pokretanju ture), čita
  `explorerPosition()` i šalje je svom modulu; `position.reload()` donosi svežu vrednost.
  Provera blizine i odluka šta raditi kad pozicije nema pripadaju modulu, a ne simulatoru.

Mapa koristi standardne OpenStreetMap raster pločice. Atribucija u donjem uglu mora
ostati vidljiva, a javni servis pločica ne sme se koristiti za masovno preuzimanje.

## Krug oko oblasti

Ulaz `circle` (`MapCircle | null`) crta najviše jedan krug: `center` (`MapCoordinate`) i
`radiusMeters` (poluprečnik u metrima). Mapa krug ne računa i ne menja; crta ga tačno onako
kako ga dobije. Kad stranica promeni centar ili poluprečnik, krug se ponovo nacrta, a kad
prosledi `null`, krug nestaje. Kad krug postoji, mapa namesti prikaz tako da se ceo krug vidi,
zajedno sa pinovima, putanjom i tačkama rute.

Krug je samo crtež preko mape: klik unutar kruga radi isto kao klik bilo gde drugde (izbor
lokacije, simulator ili ništa), a pinovi unutar kruga se i dalje mogu kliknuti.

- E23: centar je lokacija izabrana klikom (`selectionEnabled` i `locationSelected`), a
  poluprečnik stranica preračuna iz kilometara upisanih u polje forme. Dok centra ili
  ispravnog poluprečnika nema, stranica prosleđuje `null`.
- G21: stranica pri aktivaciji izazova prosledi centar i poluprečnik koje je izračunao modul;
  uz `simulatorEnabled` istraživač klikom „šeta“ po krugu.

```ts
protected readonly center = signal<MapCoordinate | null>(null);
// Poluprečnik u kilometrima, iz polja forme stranice.
protected readonly radiusKm = signal<number | null>(null);
protected readonly circle = computed<MapCircle | null>(() => {
  const center = this.center();
  const radiusKm = this.radiusKm();
  return center !== null && radiusKm !== null && radiusKm > 0
    ? { center, radiusMeters: radiusKm * 1000 }
    : null;
});
```

```html
<app-geographic-map
  [selectionEnabled]="true"
  [circle]="circle()"
  (locationSelected)="center.set($event)"
/>
```

## Ruta po ulicama

Ulaz `route` (`MapCoordinate[] | null`) su tačke rute redom. Mapa između njih nacrta jednu
liniju po ulicama, za pešaka, i javi njenu ukupnu dužinu. Rutu računa biblioteka
`leaflet-routing-machine` preko javnog pešačkog OSRM servera FOSSGIS-a
(`routing.openstreetmap.de`). Javni server je namenjen razvoju i umerenoj upotrebi, ne
produkciji. Ruta je odvojena od `path`: stranica može da koristi jedno, drugo ili oba.

- `routeLengthKm` emituje `number | null`: ukupnu dužinu rute kroz sve tačke redom, u
  kilometrima, kako je vrati server. Kada stranica prosledi `null`, linija nestaje i emituje
  se `null`, kao kod `path`.
- `routeFailed` se emituje kad server ne odgovori ili vrati grešku. Linija tada nestaje, a
  dužina se ne emituje. Tekst poruke je stranice, a ne mape.
- Sa manje od dve tačke mapa ne zove server: linije nema, a `routeLengthKm` emituje `0`. To
  nije greška.
- Server se zove samo kad stranica prosledi novu listu tačaka, nikad pri pomeranju ili
  zumiranju mape. Pri svakoj novoj listi mapa namesti prikaz tako da se vide sve tačke rute,
  zajedno sa pinovima, putanjom i krugom. Do novog ishoda ostaje stara linija. Javlja se
  samo ishod za poslednju listu; ishod ranijeg računanja koji stigne kasnije se zanemaruje.
- Na mapi je samo linija: bez markera biblioteke, bez prevlačenja i bez panela sa uputstvima.
  Linija je samo crtež, pa klik na nju radi isto kao klik na mapu.

E21: stranica od ključnih tačaka koje već ima napravi listu tačaka redom, a ispod mape
prikaže dužinu ili svoju rezervnu poruku. Poruka nestaje sa prvim sledećim uspešnim ishodom.

```ts
// Ključne tačke ture redom; stranica ih već ima.
protected readonly keyPoints = signal<MapCoordinate[]>([]);
protected readonly routeLengthKm = signal<number | null>(null);
protected readonly routeFailed = signal(false);

protected onRouteLength(lengthKm: number | null): void {
  this.routeLengthKm.set(lengthKm);
  this.routeFailed.set(false);
}
```

```html
<app-geographic-map
  [pins]="keyPoints()"
  [route]="keyPoints()"
  [selectionEnabled]="true"
  [showSelectedLocation]="false"
  (locationSelected)="keyPoints.set([...keyPoints(), $event])"
  (routeLengthKm)="onRouteLength($event)"
  (routeFailed)="routeFailed.set(true)"
/>

@if (routeFailed()) {
  <p class="error">The route could not be calculated.</p>
} @else if (routeLengthKm() !== null) {
  <p>Route length: {{ routeLengthKm()?.toFixed(1) }} km</p>
}
```

Kad autor doda, ukloni ili premesti tačku, stranica postavi nov niz (`keyPoints.set([...])`),
a mapa sama ponovo izračuna rutu. Izmena postojećeg niza na mestu se ne primećuje.

## Izbor pina

Pin (`MapPin`) može da nosi `id` koji zadaje stranica. Klik na pin sa `id` emituje
`pinSelected` sa tim identifikatorom (`string`) i ništa drugo: ne bira lokaciju i ne pomera
marker „ja“, pa se `locationSelected` i `explorerMoved` tada ne emituju. Pin bez `id` radi kao
i ranije: klik na njega je klik na mapu, pa u režimu simulatora pomera marker „ja“ na pin, a
u režimu izbora bira tu lokaciju.

G22: stranica zadrži spisak izazova koji je već pročitala i od njega napravi pinove sa `id`.
Na `pinSelected` izabere izazov iz tog spiska, bez novog zahteva serveru, i prikaže ga u
sekciji na istoj stranici, pored ili ispod mape, nikad u dijalogu ni u oblačiću na mapi. Klik
na drugi pin zameni sadržaj sekcije. Klik na pin izazova ne pomera marker „ja“; da bi se
približio izazovu, istraživač klikne pored pina.

```ts
protected readonly challenges = httpResource<ChallengeDto[]>(() => '/api/games/challenges');
protected readonly pins = computed<MapPin[]>(() =>
  (this.challenges.hasValue() ? this.challenges.value() : []).map((challenge) => ({
    id: challenge.id,
    latitude: challenge.latitude,
    longitude: challenge.longitude,
  })),
);
protected readonly selectedId = signal<string | null>(null);
protected readonly selected = computed(() =>
  this.challenges.hasValue()
    ? (this.challenges.value().find((challenge) => challenge.id === this.selectedId()) ?? null)
    : null,
);
```

```html
<app-geographic-map
  [pins]="pins()"
  [simulatorEnabled]="true"
  [explorerPosition]="explorerPosition()"
  (explorerMoved)="onExplorerMoved($event)"
  (pinSelected)="selectedId.set($event)"
/>

@if (selected(); as challenge) {
  <section class="card">
    <h2>{{ challenge.name }}</h2>
    <button type="button" class="button" (click)="activate(challenge.id)">Aktiviraj</button>
  </section>
}
```

Adresa, tipovi i imena polja u primeru su ilustracija; svaki tim koristi svoje.

## Provera na svakih 10 sekundi

Kad kartica traži da se nešto proverava u ponavljanju (E22, G22), stranica pokrene
`setInterval` i zaustavi ga sa `clearInterval`, i kad se stranica napusti
(`inject(DestroyRef).onDestroy(...)`) i kad se završi posao kome provera služi (na primer kad
sesija završi). Pozicija se uzima iz resursa stranice (`explorerPosition()` iz primera
simulatora), bez posebnog zahteva. Nova provera ne kreće dok prethodna traje. Posle komande
modula stranica ponovo učita svoj resurs.

```ts
import { Component, DestroyRef, inject, signal } from '@angular/core';

// Servis grupe stranice; ime i komanda su primer.
private readonly tourExecution = inject(TourExecution);
private intervalId: number | null = null;
protected readonly checking = signal(false);
protected readonly checkError = signal<string | null>(null);

constructor() {
  // Napuštanje stranice zaustavlja proveru.
  inject(DestroyRef).onDestroy(() => this.stopChecking());
}

// Poziva se kad sesija počne, na primer posle uspešnog pokretanja ture.
private startChecking(): void {
  this.stopChecking();
  this.intervalId = setInterval(() => this.check(), 10_000);
}

// Poziva se i kad sesija završi.
private stopChecking(): void {
  if (this.intervalId !== null) {
    clearInterval(this.intervalId);
    this.intervalId = null;
  }
}

private async check(): Promise<void> {
  const position = this.explorerPosition();
  // Dok prethodna provera traje, ili dok pozicije nema, ova se preskače.
  if (this.checking() || position === null) {
    return;
  }

  this.checking.set(true);
  this.checkError.set(null);
  try {
    await this.tourExecution.checkPosition(position);
    this.session.reload();
  } catch (failure) {
    this.checkError.set(serverMessage(failure, 'Your position could not be checked.'));
  } finally {
    this.checking.set(false);
  }
}
```

- `this.session` je resurs stranice koji prikazuje stanje posla (na primer sesiju ture); posle
  provere se ponovo učita, pa stranica prikaže ono što je modul odlučio.
- Šta raditi kad pozicije nema odlučuje tim modula; u primeru se provera tada samo preskače.
