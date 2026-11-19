using Shared.Domain.Exceptions;

namespace Shared.Domain.Conversations;

public sealed class Conversation : AggregateRoot
{
    private readonly List<ConversationParticipant> _participants = [];

    public ConversationContext Context { get; private set; }
    public IReadOnlyList<ConversationParticipant> Participants => _participants;
    public DateTime CreatedAt { get; private set; }

    private Conversation()
    {
        Context = null!;
    }

    private Conversation(ConversationContext context)
        : base(Guid.NewGuid())
    {
        Context = context;
        CreatedAt = DateTime.UtcNow;
    }

    public static Conversation CreateDirect(Guid firstUserId, Guid secondUserId)
    {
        if (firstUserId == secondUserId)
        {
            throw new DomainException("A direct conversation requires two different participants.");
        }

        var conversation = new Conversation(ConversationContext.Direct());
        conversation._participants.Add(new ConversationParticipant(firstUserId, ConversationRole.Participant));
        conversation._participants.Add(new ConversationParticipant(secondUserId, ConversationRole.Participant));
        return conversation;
    }

    public static Conversation CreateContextual(
        ConversationContext context,
        IEnumerable<Guid> privilegedUserIds)
    {
        if (context.IsDirect)
        {
            throw new DomainException("A contextual conversation requires a contextual type.");
        }

        var privilegedUsers = privilegedUserIds.Distinct().ToList();
        if (privilegedUsers.Count == 0)
        {
            throw new DomainException("A contextual conversation requires a privileged participant.");
        }

        var conversation = new Conversation(context);
        conversation._participants.AddRange(privilegedUsers
            .Select(userId => new ConversationParticipant(userId, ConversationRole.Privileged)));
        return conversation;
    }

    public void AddParticipant(Guid userId, ConversationRole role = ConversationRole.Participant)
    {
        if (Context.IsDirect)
        {
            throw new DomainException("A direct conversation cannot have another participant.");
        }
        if (FindParticipant(userId) is not null)
        {
            throw new DomainException("The user already participates in the conversation.");
        }

        _participants.Add(new ConversationParticipant(userId, role));
    }

    public bool CanBeReadBy(Guid userId) => FindParticipant(userId) is not null;

    public ChatMessage CreateMessage(Guid userId, string content, DateTime createdAt)
    {
        var participant = RequireParticipant(userId);
        if (!participant.CanSend(createdAt))
        {
            throw new DomainException("A muted participant cannot send messages.");
        }

        return new ChatMessage(Id, userId, new MessageContent(content), createdAt);
    }

    public void EnsureCanDeleteMessage(Guid userId, ChatMessage message)
    {
        var participant = RequireParticipant(userId);
        if (message.ConversationId != Id)
        {
            throw new DomainException("The message does not belong to this conversation.");
        }
        if (message.AuthorId != userId && participant.Role != ConversationRole.Privileged)
        {
            throw new DomainException("A participant cannot delete another participant's message.");
        }
    }

    public void Mute(Guid moderatorId, Guid participantId, MuteDuration duration, DateTime mutedAt)
    {
        RequirePrivilegedParticipant(moderatorId);
        RequireParticipant(participantId).ApplyMute(duration, mutedAt);
    }

    public void Unmute(Guid moderatorId, Guid participantId)
    {
        RequirePrivilegedParticipant(moderatorId);
        RequireParticipant(participantId).RemoveMute();
    }

    private void RequirePrivilegedParticipant(Guid userId)
    {
        if (RequireParticipant(userId).Role != ConversationRole.Privileged)
        {
            throw new DomainException("Only a privileged participant can moderate a conversation.");
        }
    }

    private ConversationParticipant RequireParticipant(Guid userId) =>
        FindParticipant(userId) ?? throw new DomainException("The user does not participate in the conversation.");

    private ConversationParticipant? FindParticipant(Guid userId) =>
        _participants.SingleOrDefault(participant => participant.UserId == userId);
}
