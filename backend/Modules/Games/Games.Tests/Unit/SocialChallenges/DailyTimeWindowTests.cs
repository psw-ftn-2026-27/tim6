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
}
