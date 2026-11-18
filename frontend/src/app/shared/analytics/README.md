# Deljena analitika korišćenja

`Analytics` je servis (`providedIn: 'root'`) koji drži izbor saglasnosti i jedini
razgovara sa Google Analytics 4 (GA4). Baner u `app.html`, stranica `/privacy` i
feature moduli koriste samo njega. Modul nikada ne poziva GA4 direktno (P13).

## Kada koristiš analitiku

Kad tvoj tim počinje novu funkcionalnost, proveri da li ona ima ključni korak koji
razvojni tim želi da vidi u GA4: završenu radnju korisnika (objava, kupovina, rešen
izazov) ili korak planiranog funnela (ispod). Ako ima, uz funkcionalnost ide i događaj:

1. Smisli ime po P14 i parametre po P11 i P12 (pravila su ispod).
2. Dodaj jednu metodu u `analytics.ts` i pozovi je posle uspešne komande (odeljak „Kako
   dodati događaj").
3. Proveri događaj lokalno u DebugView-u (odeljak „Lokalna provera u DebugView-u").
4. Ako događaj ima nov parametar, zamoli nekog sa Editor pristupom da ga registruje u
   GA4 (odeljak „Podešavanja u GA4"), inače se parametar ne vidi u izveštajima.

Pregled stranice ne dodaješ: servis ga beleži sam za svaku rutu, pa i za nove stranice.

## Kada je analitika uključena

Analitika radi samo kada su ispunjena oba uslova, u svakom okruženju (P2):

- okruženje ima ID merenja (`analyticsMeasurementId` u `src/environments/`), i
- korisnik je kliknuo „Accept".

Dok jedan od uslova nije ispunjen, GA4 skripta se ne učitava i ništa se ne šalje,
ni pregled prve stranice. Izbor se pamti u brauzeru; ako brauzer ne dozvoljava
pamćenje, izbor se računa kao nenapravljen i analitika ostaje isključena. Povlačenje
saglasnosti odmah zaustavlja slanje i briše kolačiće koje je GA4 upisao.

## Kako dodati događaj

1. U `analytics.ts`, u delu „The event list", dodaj jednu metodu za svoj događaj, po
   uzoru na `tourPublished`. Metoda prima samo vrednosti koje smeju da se pošalju i
   poziva `send` sa imenom događaja i parametrima. To je jedino mesto gde se imena
   događaja i parametri definišu (P14), pa spisak događaja čitaj tamo.
2. Na stranici svog modula injektuj servis i pozovi metodu tek posle uspešne komande:

```ts
private readonly analytics = inject(Analytics);

await this.clubMembership.join(clubId);
this.analytics.clubJoined(clubId);
```

Servis sam proverava saglasnost i ID merenja, pa stranica ne proverava ništa. Neuspeh
slanja je tih: stranica ne hvata greške i korisnik ništa ne primećuje. Dodavanje metode
menja deljeni kod, pa ide kroz PR koji pregleda platformski tim.

## Lokalna provera u DebugView-u

Razvojna konfiguracija (`npm start`) koristi ID property-ja „Explorer – razvoj", pa
lokalni podaci nikad ne stižu u produkcijski property.

1. Pokreni backend i `npm start`.
2. Otvori tagassistant.google.com, klikni „Add domain" i unesi `http://localhost:4200`.
   Aplikacija se otvara u novom prozoru, sa dodatkom `?gtm_debug=...` u adresi. Proveru
   radi u tom prozoru: DebugView prikazuje samo saobraćaj iz prozora povezanog sa Tag
   Assistant-om.
3. Pre klika na „Accept" Tag Assistant ne nalazi nijedan tag. To je očekivano: bez
   saglasnosti se GA4 skripta ne učitava (P1).
4. Klikni „Accept", pa pređi na drugu stranicu. Stranica na kojoj si kliknuo „Accept" se
   ne šalje; prvi `page_view` je sledeća promena rute. Zatim uradi radnju koju
   proveravaš.
5. U GA4 otvori property „Explorer – razvoj" → Admin → DebugView. Događaj se pojavljuje
   za nekoliko sekundi. Klikni ga i proveri parametre: `role`, `page_location` sa šablonom
   rute i parametre tvog događaja, bez ijednog podatka koji pravila zabranjuju.

Ako DebugView ostane prazan:

- Gore levo, u listi „Debug devices", izaberi najnoviji uređaj. Posle „Reject" i
  ponovnog „Accept" GA4 te vidi kao novog posetioca, pa se događaji pojavljuju pod novim
  uređajem, a DebugView i dalje prikazuje stari.
- Ako je veza sa Tag Assistant-om prekinuta, klikni „Finish" pa ponovo „Connect" i radi
  u novom prozoru.

Bez Tag Assistant-a: Reports → Realtime prikazuje sve događaje iz poslednjih 30 minuta,
a klik na ime događaja prikazuje njegove parametre. U mrežnoj kartici brauzera vidiš da
pre saglasnosti nema zahteva ka `googletagmanager.com` ni `google-analytics.com`.

## Podešavanja u GA4

- Postoje dva property-ja: „Explorer – razvoj" (ID u `environment.development.ts`) i
  „Explorer – produkcija" (ID u `environment.ts`). Odvojeni su da se razvojni podaci ne
  mešaju sa stvarnim (P3).
- U oba web stream-a je isključen Enhanced measurement. Uključen bi sam slao preglede
  stranica sa pravom adresom, pretragu iz parametara upita i interakcije sa formama, što
  krši P8 i P12. Ne uključuj ga.
- Parametar događaja se u izveštajima i funnelu vidi tek kada je registrovan kao
  dimenzija: Admin → Custom definitions → Create custom dimension, opseg „Event", u oba
  property-ja. U DebugView-u se vidi i bez toga.
- Pristup: platformski tim ima Editor, ostali timovi Viewer. Za novu dimenziju ili
  funnel javi se platformskom timu.

## Pravila o podacima

- P8. Pregled stranice beleži se pri svakoj promeni rute, kao šablon rute (npr.
  `/payment/wallets/:id`), bez konkretnih identifikatora i bez parametara upita. Servis
  to radi sam; modul ne šalje preglede stranica.
- P9. Uz svaki događaj i pregled servis dodaje ulogu: sve uloge korisnika, sortirane i
  spojene zarezom, a `guest` za neulogovanog korisnika. Danas sistem ima uloge
  `administrator` i `explorer`, pa vrednosti izgledaju ovako: `explorer`,
  `administrator`, `guest` (ili `administrator,explorer` za korisnika sa obe uloge).
- P10. Nijedan događaj ne sadrži ime, prezime, mejl, korisničko ime ni identifikator
  korisnika; GA4 se ne podešava sa User-ID vrednošću.
- P11. Parametar sme da bude identifikator entiteta koji nije vezan za jednog korisnika
  (tura, klub, izazov). Identifikator entiteta koji pripada tačno jednom korisniku (npr.
  novčanik) se ne šalje.
- P12. Bez tokena, sesijskih podataka, finansijskih podataka korisnika (npr. stanje
  novčanika) i slobodnog teksta koji korisnik unosi.
- P13. Moduli beleže događaje samo kroz ovaj servis.
- P14. Ime događaja ima oblik `<entitet>_<radnja u prošlom vremenu>`, malim slovima sa
  donjom crtom (npr. `tour_published`, `challenge_solved`). Ime parametra je takođe
  malim slovima sa donjom crtom (npr. `tour_id`).

## Planirani funnel

Funnel u GA4 ima tri koraka:

1. pregled ture,
2. dodavanje u korpu,
3. kupovina.

Događaje ovih koraka dodaje tim koji pravi tu funkcionalnost (Exploration za stranicu
ture, Payment za korpu i kupovinu), po uputstvu iznad. Funnel se podešava u GA4 i
proverava kada svi koraci postanu dostupni.
