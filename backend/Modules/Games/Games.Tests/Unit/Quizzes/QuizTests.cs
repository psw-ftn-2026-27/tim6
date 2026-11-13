using FluentAssertions;
using Games.Domain.Challenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private static Question CreateQuestion() =>
        new("Kako se zove čuveni sat na tvrđavi?", [new AnswerOption("Pijani sat", true), new AnswerOption("Zlatni sat", false)]);

    private static List<Question> CreateQuestions(int count) =>
        Enumerable.Range(0, count).Select(_ => CreateQuestion()).ToList();

    private static Quiz CreateQuiz() =>
        new("Istorija tvrđave", "Prošetajte tvrđavom.", 100, CreateQuestions(1));

    private static Quiz CreateLocatedQuiz()
    {
        var quiz = CreateQuiz();
        quiz.PlaceAt(new Location(45.2517, 19.8619));
        return quiz;
    }

    private static Quiz CreatePublishedQuiz()
    {
        var quiz = CreateLocatedQuiz();
        quiz.Publish();
        return quiz;
    }

    private static Quiz CreateRetiredQuiz()
    {
        var quiz = CreatePublishedQuiz();
        quiz.Retire();
        return quiz;
    }

    private static Quiz CreateArchivedQuiz()
    {
        var quiz = CreateRetiredQuiz();
        quiz.Archive();
        return quiz;
    }

    [Fact]
    public void New_quiz_is_a_draft()
    {
        var quiz = CreateQuiz();

        quiz.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_without_a_name_is_rejected(string name)
    {
        var creation = () => new Quiz(name, "Prošetajte tvrđavom.", 100, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_with_a_name_of_two_hundred_characters_is_created()
    {
        var name = new string('a', 200);

        var quiz = new Quiz(name, "Prošetajte tvrđavom.", 100, CreateQuestions(1));

        quiz.Name.Should().Be(name);
    }

    [Fact]
    public void Quiz_name_longer_than_two_hundred_characters_is_rejected()
    {
        var creation = () => new Quiz(new string('a', 201), "Prošetajte tvrđavom.", 100, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Quiz_without_instructions_is_rejected(string instructions)
    {
        var creation = () => new Quiz("Istorija tvrđave", instructions, 100, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_without_xp_is_rejected()
    {
        var creation = () => new Quiz("Istorija tvrđave", "Prošetajte tvrđavom.", 0, CreateQuestions(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_rewarding_one_xp_is_created()
    {
        var quiz = new Quiz("Istorija tvrđave", "Prošetajte tvrđavom.", 1, CreateQuestions(1));

        quiz.Xp.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Quiz_requires_between_one_and_twenty_questions(int questionCount)
    {
        var creation = () => new Quiz("Istorija tvrđave", "Prošetajte tvrđavom.", 100, CreateQuestions(questionCount));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void Quiz_with_one_or_twenty_questions_is_created(int questionCount)
    {
        var quiz = new Quiz("Istorija tvrđave", "Prošetajte tvrđavom.", 100, CreateQuestions(questionCount));

        quiz.Questions.Should().HaveCount(questionCount);
    }

    [Fact]
    public void Draft_edited_with_invalid_data_keeps_its_previous_data()
    {
        var quiz = CreateQuiz();
        var questionsBefore = quiz.Questions.ToList();

        var edit = () => quiz.Edit("", "Nova uputstva.", 200, CreateQuestions(2));

        edit.Should().Throw<DomainException>();
        quiz.Name.Should().Be("Istorija tvrđave");
        quiz.Instructions.Should().Be("Prošetajte tvrđavom.");
        quiz.Xp.Should().Be(100);
        quiz.Questions.Should().Equal(questionsBefore);
    }

    [Fact]
    public void Published_quiz_cannot_be_moved()
    {
        var quiz = CreatePublishedQuiz();

        var placing = () => quiz.PlaceAt(new Location(44.8125, 20.4612));

        placing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_quiz_cannot_lose_its_location()
    {
        var quiz = CreatePublishedQuiz();

        var removal = () => quiz.RemoveLocation();

        removal.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_with_a_location_is_published()
    {
        var quiz = CreateLocatedQuiz();

        quiz.Publish();

        quiz.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Retired_quiz_is_published_again()
    {
        var quiz = CreateRetiredQuiz();

        quiz.Publish();

        quiz.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Archived_quiz_cannot_be_published()
    {
        var quiz = CreateArchivedQuiz();

        var publishing = () => quiz.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_without_a_location_cannot_be_published()
    {
        var quiz = CreateQuiz();

        var publishing = () => quiz.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_cannot_be_retired()
    {
        var quiz = CreateLocatedQuiz();

        var retiring = () => quiz.Retire();

        retiring.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_quiz_cannot_be_archived()
    {
        var quiz = CreatePublishedQuiz();

        var archiving = () => quiz.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_quiz_can_be_deleted()
    {
        var quiz = CreateArchivedQuiz();

        var deletion = () => quiz.Delete();

        deletion.Should().NotThrow();
    }

    [Fact]
    public void Published_quiz_cannot_be_deleted()
    {
        var quiz = CreatePublishedQuiz();

        var deletion = () => quiz.Delete();

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_quiz_cannot_be_edited()
    {
        var quiz = CreatePublishedQuiz();

        var edit = () => quiz.Edit("Istorija tvrđave", "Prošetajte tvrđavom.", 100, CreateQuestions(1));

        edit.Should().Throw<DomainException>();
    }
}
