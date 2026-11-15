# Deljena analitika korišćenja

`Analytics` je servis (`providedIn: 'root'`) koji drži izbor saglasnosti i jedini
razgovara sa Google Analytics 4 (GA4). Baner u `app.html`, stranica `/privacy` i
feature moduli koriste samo njega. Modul nikada ne poziva GA4 direktno (P13).

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

## Pravila o podacima

- P8. Pregled stranice beleži se pri svakoj promeni rute, kao šablon rute (npr.
  `/payment/wallets/:id`), bez konkretnih identifikatora i bez parametara upita. Servis
  to radi sam; modul ne šalje preglede stranica.
- P9. Uz svaki događaj i pregled servis dodaje ulogu: sve uloge korisnika, sortirane i
  spojene zarezom (npr. `author,tourist`), a `guest` za neulogovanog korisnika.
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
