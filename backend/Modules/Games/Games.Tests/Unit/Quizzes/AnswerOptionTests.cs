using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class AnswerOptionTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Answer_option_without_a_text_is_rejected(string text)
    {
        var creation = () => new AnswerOption(text, true);

        creation.Should().Throw<DomainException>();
    }
}
