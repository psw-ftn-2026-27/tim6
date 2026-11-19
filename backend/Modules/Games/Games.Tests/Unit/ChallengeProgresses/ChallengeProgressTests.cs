using FluentAssertions;
using Games.Domain.ChallengeProgresses;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.ChallengeProgresses;

public class ChallengeProgressTests
{
    private static readonly DateTime RecordedAt = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

    private static ChallengeProgress CreateProgressWithEarnedXp(int earnedXp)
    {
        var progress = new ChallengeProgress(Guid.NewGuid(), Guid.NewGuid());
        progress.RecordResult(earnedXp, false, RecordedAt);
        return progress;
    }

    [Theory]
    [InlineData(40, 70, 30, 70)]
    [InlineData(70, 40, 0, 70)]
    [InlineData(70, 70, 0, 70)]
    [InlineData(0, 100, 100, 100)]
    public void Attempt_awards_only_the_xp_above_what_was_already_earned(int earnedXp, int attemptXp,
        int expectedAwardedXp, int expectedEarnedXp)
    {
        var progress = CreateProgressWithEarnedXp(earnedXp);

        var awardedXp = progress.RecordResult(attemptXp, false, RecordedAt);

        awardedXp.Should().Be(expectedAwardedXp);
        progress.EarnedXp.Should().Be(expectedEarnedXp);
    }

    [Fact]
    public void Solved_challenge_rejects_another_result()
    {
        var progress = new ChallengeProgress(Guid.NewGuid(), Guid.NewGuid());
        progress.RecordResult(100, true, RecordedAt);

        var recording = () => progress.RecordResult(100, true, RecordedAt);

        recording.Should().Throw<DomainException>();
    }

    [Fact]
    public void Attempt_with_every_answer_correct_solves_the_challenge_and_records_the_moment()
    {
        var progress = CreateProgressWithEarnedXp(50);

        progress.RecordResult(100, true, RecordedAt);

        progress.IsSolved().Should().BeTrue();
        progress.SolvedAt.Should().Be(RecordedAt);
    }
}
