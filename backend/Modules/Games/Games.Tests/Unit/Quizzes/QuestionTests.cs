using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuestionTests
{
    private static List<AnswerOption> CreateOptions(int count) =>
        Enumerable.Range(1, count).Select(number => new AnswerOption($"Odgovor {number}", number == 1)).ToList();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Question_without_a_text_is_rejected(string text)
    {
        var creation = () => new Question(text, CreateOptions(2));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void Question_requires_between_two_and_six_answer_options(int optionCount)
    {
        var creation = () => new Question("Koje godine je počela izgradnja tvrđave?", CreateOptions(optionCount));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(6)]
    public void Question_with_two_or_six_answer_options_is_created(int optionCount)
    {
        var question = new Question("Koje godine je počela izgradnja tvrđave?", CreateOptions(optionCount));

        question.Options.Should().HaveCount(optionCount);
    }

    [Fact]
    public void Question_with_answer_options_differing_only_in_surrounding_spaces_and_case_is_rejected()
    {
        var options = new List<AnswerOption> { new("Knez Mihailova", true), new(" knez mihailova ", false) };

        var creation = () => new Question("Koja ulica je glavna pešačka zona?", options);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public void Question_requires_exactly_one_correct_answer_option(bool firstIsCorrect, bool secondIsCorrect)
    {
        var options = new List<AnswerOption> { new("1692", firstIsCorrect), new("1780", secondIsCorrect) };

        var creation = () => new Question("Koje godine je počela izgradnja tvrđave?", options);

        creation.Should().Throw<DomainException>();
    }
}
