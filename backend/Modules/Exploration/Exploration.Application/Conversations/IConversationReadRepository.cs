namespace Exploration.Application.Conversations;

public interface IConversationReadRepository
{
    Task<ConversationDto?> GetAsync(Guid conversationId);
    Task<MessagePageDto> GetMessagesAsync(
        Guid conversationId, MessageCursorDto? cursor, bool afterCursor, int pageSize);
}
