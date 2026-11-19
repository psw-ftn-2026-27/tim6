using Shared.Domain.Exceptions;

namespace Shared.Domain.Conversations;

public sealed class ChatMessage : AggregateRoot
{
    public Guid ConversationId { get; private set; }
    public Guid AuthorId { get; private set; }
    public MessageContent? Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public bool IsDeleted => DeletedAt is not null;

    private ChatMessage() { }

    internal ChatMessage(Guid conversationId, Guid authorId, MessageContent content, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        ConversationId = conversationId;
        AuthorId = authorId;
        Content = content;
        CreatedAt = createdAt;
    }

    public void Delete(DateTime deletedAt)
    {
        if (IsDeleted)
        {
            throw new DomainException("A deleted message cannot be deleted again.");
        }

        Content = null;
        DeletedAt = deletedAt;
    }
}
