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

    public static Quiz[] All => [DraftFortressHistory, DraftCityCenter];
}
