using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class DailyTimeWindowTests
{
    [Fact]
    public void Daily_time_window_cannot_start_and_end_at_the_same_time()
    {
        var creation = () => new DailyTimeWindow(new TimeOnly(10, 0), new TimeOnly(10, 0));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Daily_time_window_may_cross_midnight_and_lasts_until_the_next_day()
    {
        var timeWindow = new DailyTimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0));

        timeWindow.Duration.Should().Be(TimeSpan.FromHours(4));
    }

    [Theory]
    [InlineData(10, 0, 10, 59)]
    [InlineData(10, 0, 16, 1)]
    [InlineData(23, 0, 23, 30)]
    [InlineData(21, 0, 3, 1)]
    public void Daily_time_window_lasts_between_one_and_six_hours(int startHour, int startMinute, int endHour, int endMinute)
    {
        var creation = () => new DailyTimeWindow(new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(10, 0, 11, 0, 1)]
    [InlineData(10, 0, 16, 0, 6)]
    [InlineData(23, 30, 0, 30, 1)]
    [InlineData(21, 0, 3, 0, 6)]
    public void Daily_time_window_of_exactly_one_or_six_hours_is_accepted(int startHour, int startMinute, int endHour, int endMinute, int hours)
    {
        var timeWindow = new DailyTimeWindow(new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute));

        timeWindow.Duration.Should().Be(TimeSpan.FromHours(hours));
    }

    [Theory]
    [InlineData(18, 0, true)]
    [InlineData(19, 59, true)]
    [InlineData(20, 0, false)]
    [InlineData(17, 59, false)]
    public void Daily_time_window_holds_its_start_but_not_its_end(int hour, int minute, bool contained)
    {
        var timeWindow = new DailyTimeWindow(new TimeOnly(18, 0), new TimeOnly(20, 0));

        var contains = timeWindow.Contains(new TimeOnly(hour, minute));

        contains.Should().Be(contained);
    }

    [Theory]
    [InlineData(23, 30, true)]
    [InlineData(0, 0, true)]
    [InlineData(1, 30, true)]
    [InlineData(2, 0, false)]
    [InlineData(12, 0, false)]
    [InlineData(21, 59, false)]
    public void Daily_time_window_across_midnight_holds_moments_before_and_after_midnight_only(int hour, int minute, bool contained)
    {
        var timeWindow = new DailyTimeWindow(new TimeOnly(22, 0), new TimeOnly(2, 0));

        var contains = timeWindow.Contains(new TimeOnly(hour, minute));

        contains.Should().Be(contained);
    }
}
