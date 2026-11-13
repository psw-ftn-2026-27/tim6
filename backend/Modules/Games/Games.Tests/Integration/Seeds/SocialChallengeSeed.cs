using Games.Domain.Challenges;
using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge DraftSquareGathering = new("Okupljanje na trgu", new DailyTimeWindow(new TimeOnly(18, 0), new TimeOnly(20, 0)), 5, 50, 100, "Svi ste se okupili na trgu!");
    public static readonly SocialChallenge DraftMidnightWalk = new("Ponoćna šetnja", new DailyTimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0)), 10, 200, 250, "Prošetali ste zajedno kroz ponoć!");
    public static readonly SocialChallenge LocatedDraftFortressSunset;
    public static readonly SocialChallenge PublishedRiversidePicnic;
    public static readonly SocialChallenge RetiredWinterSkating;
    public static readonly SocialChallenge ArchivedHarvestFestival;

    static SocialChallengeSeed()
    {
        LocatedDraftFortressSunset = new("Zalazak sunca na tvrđavi", new DailyTimeWindow(new TimeOnly(19, 0), new TimeOnly(21, 0)), 6, 80, 150, "Zajedno ste ispratili sunce!");
        LocatedDraftFortressSunset.PlaceAt(new Location(45.2517, 19.8619));

        PublishedRiversidePicnic = new("Piknik na keju", new DailyTimeWindow(new TimeOnly(12, 0), new TimeOnly(15, 0)), 4, 100, 120, "Uživali ste u pikniku na keju!");
        PublishedRiversidePicnic.PlaceAt(new Location(45.2551, 19.8547));
        PublishedRiversidePicnic.Publish();

        RetiredWinterSkating = new("Zimsko klizanje", new DailyTimeWindow(new TimeOnly(16, 0), new TimeOnly(19, 0)), 8, 60, 200, "Zajedno ste klizali!");
        RetiredWinterSkating.PlaceAt(new Location(45.2464, 19.8425));
        RetiredWinterSkating.Publish();
        RetiredWinterSkating.Retire();

        ArchivedHarvestFestival = new("Festival berbe", new DailyTimeWindow(new TimeOnly(10, 0), new TimeOnly(14, 0)), 12, 250, 300, "Proslavili ste berbu zajedno!");
        ArchivedHarvestFestival.PlaceAt(new Location(45.1580, 19.9310));
        ArchivedHarvestFestival.Publish();
        ArchivedHarvestFestival.Retire();
        ArchivedHarvestFestival.Archive();
    }

    public static SocialChallenge[] All => [DraftSquareGathering, DraftMidnightWalk, LocatedDraftFortressSunset, PublishedRiversidePicnic, RetiredWinterSkating, ArchivedHarvestFestival];
}
