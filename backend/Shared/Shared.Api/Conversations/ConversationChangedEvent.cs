namespace Shared.Api.Conversations;

public sealed record ConversationChangedEvent(Guid ConversationId, ConversationChangeType ChangeType);
