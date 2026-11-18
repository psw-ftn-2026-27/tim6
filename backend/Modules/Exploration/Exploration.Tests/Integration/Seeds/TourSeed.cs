using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourSeed
{
    public static readonly Tour FortressWalk = new(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav.", TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(2, 6));
    public static readonly Tour CityStroll = new(WellKnownUsers.Explorer, "Šetnja centrom", "Kratak opis gradske šetnje.", TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(2, 6));
    public static readonly Tour SecondExplorersTrail = new(WellKnownUsers.SecondExplorer, "Tuđa staza", "Kratak opis staze drugog autora.", TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(2, 6));
    public static readonly Tour PublishableRiverside;
    public static readonly Tour PublishedVineyards;
    public static readonly Tour PublishedMonasteries;
    public static readonly Tour ArchivedSalashes;

    static TourSeed()
    {
        PublishableRiverside = new(WellKnownUsers.Explorer, "Staza uz Dunav", "Staza kreće od Ribarskog ostrva, prati obalu Dunava pored gradske plaže Štrand i završava se kod Mosta slobode, uz više mesta za predah.", TourDifficulty.Easy, ["priroda"], new RecommendedGroupSize(2, 6));
        PublishableRiverside.AddTransportTime(TransportMode.Walking, 90);
        PublishableRiverside.AddKeyPoint("Ribarsko ostrvo", "Polazna tačka staze, uz ribarske čamce.", "/api/shared/images/ribarsko-ostrvo.jpg", null, new Location(45.2322, 19.8930), true, new TourLength(0));
        PublishableRiverside.AddKeyPoint("Štrand", "Gradska plaža sa mestom za predah.", "/api/shared/images/strand.jpg", "Ispod mosta je stari natpis.", new Location(45.2390, 19.8590), false, new TourLength(3.1));
        PublishableRiverside.AddKeyPoint("Most slobode", "Kraj staze, kod Mosta slobode.", "/api/shared/images/most-slobode.jpg", null, new Location(45.2420, 19.8450), false, new TourLength(4.2));

        PublishedVineyards = new(WellKnownUsers.Explorer, "Vinogradi Sremskih Karlovaca", "Tura vodi kroz vinograde na obroncima Fruške gore, uz obilazak dva porodična podruma i degustaciju bermeta u Sremskim Karlovcima.", TourDifficulty.Moderate, ["vino", "priroda"], new RecommendedGroupSize(2, 6));
        PublishedVineyards.AddTransportTime(TransportMode.Bicycle, 60);
        PublishedVineyards.AddKeyPoint("Trg Branka Radičevića", "Početak ture u centru Sremskih Karlovaca.", "/api/shared/images/karlovci-trg.jpg", null, new Location(45.2030, 19.9330), true, new TourLength(0));
        PublishedVineyards.AddKeyPoint("Porodični podrum", "Degustacija bermeta u porodičnom podrumu.", "/api/shared/images/podrum.jpg", null, new Location(45.1980, 19.9280), false, new TourLength(1.5));
        PublishedVineyards.Publish(new TourPrice(true, null));

        PublishedMonasteries = new(WellKnownUsers.Explorer, "Fruškogorski manastiri", "Tura obilazi tri fruškogorska manastira: Krušedol, Grgeteg i Novo Hopovo, sa pauzom za ručak u podnožju nacionalnog parka.", TourDifficulty.Hard, ["istorija"], new RecommendedGroupSize(2, 6));
        PublishedMonasteries.AddTransportTime(TransportMode.Car, 45);
        PublishedMonasteries.AddKeyPoint("Krušedol", "Manastir iz šesnaestog veka.", "/api/shared/images/krusedol.jpg", null, new Location(45.1190, 19.9420), true, new TourLength(0));
        PublishedMonasteries.AddKeyPoint("Grgeteg", "Manastir na južnim obroncima Fruške gore.", "/api/shared/images/grgeteg.jpg", null, new Location(45.1390, 19.8780), false, new TourLength(8.7));
        PublishedMonasteries.Publish(new TourPrice(true, null));

        ArchivedSalashes = new(WellKnownUsers.Explorer, "Bački salaši", "Tura obilazi tri salaša u okolini Novog Sada, uz razgledanje starih zanata, vožnju fijakerom i ručak od domaćih namirnica na poslednjem salašu.", TourDifficulty.Easy, ["selo", "hrana"], new RecommendedGroupSize(2, 6));
        ArchivedSalashes.AddTransportTime(TransportMode.Car, 40);
        ArchivedSalashes.AddKeyPoint("Prvi salaš", "Razgledanje starih zanata.", "/api/shared/images/prvi-salas.jpg", null, new Location(45.2950, 19.7800), true, new TourLength(0));
        ArchivedSalashes.AddKeyPoint("Poslednji salaš", "Ručak od domaćih namirnica.", "/api/shared/images/poslednji-salas.jpg", null, new Location(45.3100, 19.7500), false, new TourLength(5.6));
        ArchivedSalashes.Publish(new TourPrice(true, null));
        ArchivedSalashes.Archive();
    }

    public static Tour[] All => [FortressWalk, CityStroll, SecondExplorersTrail, PublishableRiverside, PublishedVineyards, PublishedMonasteries, ArchivedSalashes];
}
