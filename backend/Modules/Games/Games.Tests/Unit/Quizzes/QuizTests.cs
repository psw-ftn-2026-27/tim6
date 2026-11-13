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
}
