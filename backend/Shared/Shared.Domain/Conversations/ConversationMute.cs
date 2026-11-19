using Shared.Domain.Exceptions;

namespace Shared.Domain.Conversations;

public sealed record ConversationMute
{
    public DateTime? EndsAt { get; private init; }
    public bool IsPermanent { get; private init; }

    private ConversationMute() { }

    private ConversationMute(DateTime? endsAt, bool isPermanent)
    {
        EndsAt = endsAt;
        IsPermanent = isPermanent;
    }

    public static ConversationMute Create(MuteDuration duration, DateTime now) => duration switch
    {
        MuteDuration.FifteenMinutes => new(now.AddMinutes(15), false),
        MuteDuration.OneHour => new(now.AddHours(1), false),
        MuteDuration.TwentyFourHours => new(now.AddHours(24), false),
        MuteDuration.Permanent => new(null, true),
        _ => throw new DomainException("The mute duration is not supported.")
    };

    public bool IsActiveAt(DateTime now) => IsPermanent || EndsAt > now;
}
