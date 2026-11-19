using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record DailyTimeWindow
{
    private static readonly TimeSpan MinDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaxDuration = TimeSpan.FromHours(6);

    public TimeOnly Start { get; private init; }
    public TimeOnly End { get; private init; }

    // Subtracting two TimeOnly values wraps around midnight, so 22:00 to 02:00 lasts 4 hours.
    public TimeSpan Duration => End - Start;

    private DailyTimeWindow() { }

    public DailyTimeWindow(TimeOnly start, TimeOnly end)
    {
        if (start == end)
        {
            throw new DomainException("A daily time window requires different start and end times.");
        }

        var duration = end - start;
        if (duration < MinDuration)
        {
            throw new DomainException($"A daily time window lasts at least {MinDuration.TotalHours} hour.");
        }
        if (duration > MaxDuration)
        {
            throw new DomainException($"A daily time window lasts at most {MaxDuration.TotalHours} hours.");
        }

        Start = start;
        End = end;
    }

    // The start belongs to the window and the end does not; a window that crosses midnight
    // holds the moments before and after it.
    public bool Contains(TimeOnly moment)
    {
        return moment.IsBetween(Start, End);
    }

    public bool IsOpenAt(DateTime utcMoment)
    {
        return Contains(TimeOnly.FromDateTime(CityTime.FromUtc(utcMoment)));
    }

    // Whether the window that was open at the start has ended by the moment. A run starts inside
    // the window, so its window ends at the first end time after the start; subtracting two
    // TimeOnly values wraps around midnight.
    public bool HasEndedSince(DateTime utcStart, DateTime utcMoment)
    {
        var start = CityTime.FromUtc(utcStart);
        var endsAt = start + (End - TimeOnly.FromDateTime(start));
        return CityTime.FromUtc(utcMoment) >= endsAt;
    }
}
