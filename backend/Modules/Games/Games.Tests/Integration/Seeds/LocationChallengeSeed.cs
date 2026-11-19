using Games.Domain.Challenges;
using Games.Domain.LocationChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge DraftHiddenFacades = new("Skrivene fasade", 120,
    [
        new HiddenPoint("/api/shared/images/fasada-1.jpg", new Location(45.2553, 19.8449)),
        new HiddenPoint("/api/shared/images/fasada-2.jpg", new Location(45.2561, 19.8432))
    ]);

    public static readonly LocationChallenge LocatedDraftFortressCorners;
    public static readonly LocationChallenge PublishedRiversideDetails;
    public static readonly LocationChallenge RetiredParkSculptures;
    public static readonly LocationChallenge ArchivedOldGates;

    static LocationChallengeSeed()
    {
        LocatedDraftFortressCorners = new("Uglovi tvrđave", 150,
        [
            new HiddenPoint("/api/shared/images/tvrdjava-1.jpg", new Location(45.2522, 19.8628)),
            new HiddenPoint("/api/shared/images/tvrdjava-2.jpg", new Location(45.2509, 19.8641))
        ]);
        LocatedDraftFortressCorners.PlaceAt(new Location(45.2517, 19.8619));

        PublishedRiversideDetails = new("Detalji sa keja", 100,
        [
            new HiddenPoint("/api/shared/images/kej-1.jpg", new Location(45.2548, 19.8552))
        ]);
        PublishedRiversideDetails.PlaceAt(new Location(45.2551, 19.8547));
        PublishedRiversideDetails.Publish();

        RetiredParkSculptures = new("Skulpture u parku", 90,
        [
            new HiddenPoint("/api/shared/images/park-1.jpg", new Location(45.2543, 19.8505)),
            new HiddenPoint("/api/shared/images/park-2.jpg", new Location(45.2538, 19.8514))
        ]);
        RetiredParkSculptures.PlaceAt(new Location(45.2541, 19.8509));
        RetiredParkSculptures.Publish();
        RetiredParkSculptures.Retire();

        ArchivedOldGates = new("Stare kapije", 60,
        [
            new HiddenPoint("/api/shared/images/kapija-1.jpg", new Location(45.2530, 19.8601))
        ]);
        ArchivedOldGates.PlaceAt(new Location(45.2528, 19.8597));
        ArchivedOldGates.Publish();
        ArchivedOldGates.Retire();
        ArchivedOldGates.Archive();
    }

    public static LocationChallenge[] All => [DraftHiddenFacades, LocatedDraftFortressCorners, PublishedRiversideDetails, RetiredParkSculptures, ArchivedOldGates];
}
