using Games.Domain.Challenges;
using Games.Domain.Quizzes;

namespace Games.Tests.Integration.Seeds;

internal static class QuizSeed
{
    public static readonly Quiz DraftFortressHistory = new("Istorija tvrđave", "Prošetajte tvrđavom i odgovorite na pitanja o njenoj prošlosti.", 150,
    [
        new Question("Koje godine je počela izgradnja tvrđave?", [new AnswerOption("1692", true), new AnswerOption("1780", false), new AnswerOption("1848", false)]),
        new Question("Kako se zove čuveni sat na tvrđavi?", [new AnswerOption("Pijani sat", true), new AnswerOption("Zlatni sat", false)])
    ]);

    public static readonly Quiz DraftCityCenter = new("Centar grada", "Pronađite odgovore šetajući kroz centar grada.", 80,
    [
        new Question("Koja ulica je glavna pešačka zona?", [new AnswerOption("Zmaj Jovina", true), new AnswerOption("Bulevar oslobođenja", false), new AnswerOption("Futoška", false), new AnswerOption("Kisačka", false)])
    ]);

    public static readonly Quiz LocatedDraftDanubeBridges;
    public static readonly Quiz PublishedOldTown;
    public static readonly Quiz RetiredRiversideParks;
    public static readonly Quiz ArchivedWinterFair;

    static QuizSeed()
    {
        LocatedDraftDanubeBridges = new("Mostovi na Dunavu", "Prošetajte kejom i odgovorite na pitanja o mostovima.", 100,
        [
            new Question("Koji most povezuje centar sa tvrđavom?", [new AnswerOption("Varadinski most", true), new AnswerOption("Most slobode", false)])
        ]);
        LocatedDraftDanubeBridges.PlaceAt(new Location(45.2532, 19.8636));

        PublishedOldTown = new("Stari grad", "Obiđite stari grad i odgovorite na pitanja.", 120,
        [
            new Question("Kako se zove glavni trg?", [new AnswerOption("Trg slobode", true), new AnswerOption("Trg mladenaca", false)])
        ]);
        PublishedOldTown.PlaceAt(new Location(45.2551, 19.8451));
        PublishedOldTown.Publish();

        RetiredRiversideParks = new("Parkovi uz reku", "Prošetajte parkovima i odgovorite na pitanja.", 90,
        [
            new Question("Koji park je najstariji u gradu?", [new AnswerOption("Dunavski park", true), new AnswerOption("Limanski park", false)])
        ]);
        RetiredRiversideParks.PlaceAt(new Location(45.2541, 19.8509));
        RetiredRiversideParks.Publish();
        RetiredRiversideParks.Retire();

        ArchivedWinterFair = new("Zimski sajam", "Obiđite sajam i odgovorite na pitanja.", 60,
        [
            new Question("Gde se održava zimski sajam?", [new AnswerOption("Na trgu", true), new AnswerOption("Na keju", false)])
        ]);
        ArchivedWinterFair.PlaceAt(new Location(45.2557, 19.8448));
        ArchivedWinterFair.Publish();
        ArchivedWinterFair.Retire();
        ArchivedWinterFair.Archive();
    }

    public static Quiz[] All => [DraftFortressHistory, DraftCityCenter, LocatedDraftDanubeBridges, PublishedOldTown, RetiredRiversideParks, ArchivedWinterFair];
}
