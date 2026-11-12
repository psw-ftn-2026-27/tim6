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
}
