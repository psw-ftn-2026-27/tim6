using Shared.Domain.Exceptions;

namespace Shared.Domain.Conversations;

public sealed record ConversationContext
{
    private const string DirectType = "direct";

    public string Type { get; private init; }
    public Guid? ContextId { get; private init; }
    public bool IsDirect => Type == DirectType;

    private ConversationContext()
    {
        Type = null!;
    }

    private ConversationContext(string type, Guid? contextId)
    {
        Type = type;
        ContextId = contextId;
    }

    public static ConversationContext Direct() => new(DirectType, null);

    public static ConversationContext Contextual(string type, Guid contextId)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new DomainException("A contextual conversation requires a context type.");
        }
        if (string.Equals(type, DirectType, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("The direct conversation type is reserved.");
        }
        if (contextId == Guid.Empty)
        {
            throw new DomainException("A contextual conversation requires a context identifier.");
        }

        return new ConversationContext(type.Trim(), contextId);
    }
}
