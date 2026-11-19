using Shared.Domain.Exceptions;

namespace Shared.Domain.Conversations;

public sealed class ConversationParticipant : Entity
{
    public Guid UserId { get; private set; }
    public ConversationRole Role { get; private set; }
    public ConversationMute? Mute { get; private set; }

    private ConversationParticipant() { }

    internal ConversationParticipant(Guid userId, ConversationRole role)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        Role = role;
    }

    internal void ApplyMute(MuteDuration duration, DateTime now)
    {
        if (Role == ConversationRole.Privileged)
        {
            throw new DomainException("A privileged participant cannot be muted.");
        }

        Mute = ConversationMute.Create(duration, now);
    }

    internal void RemoveMute()
    {
        if (Mute is null)
        {
            throw new DomainException("The participant is not muted.");
        }

        Mute = null;
    }

    public bool CanSend(DateTime now) => Mute is null || !Mute.IsActiveAt(now);
}
