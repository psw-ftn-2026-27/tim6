using Shared.Domain;

namespace Social.Application.Conversations;

public interface IConversationReadRepository
{
    Task<PageResult<ConversationSummaryDto>> GetMineAsync(Guid userId, int page, int pageSize);
    Task<ConversationDto?> GetAsync(Guid conversationId);
    Task<MessagePageDto> GetMessagesAsync(
        Guid conversationId, MessageCursorDto? cursor, bool afterCursor, int pageSize);
}
