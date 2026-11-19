using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Shared.Domain.Conversations;
using Social.Application.Conversations;

namespace Social.Infrastructure.Persistence.Conversations;

internal sealed class ConversationReadRepository : IConversationReadRepository
{
    private readonly SocialDbContext _dbContext;
    public ConversationReadRepository(SocialDbContext dbContext) => _dbContext = dbContext;

    public async Task<PageResult<ConversationSummaryDto>> GetMineAsync(Guid userId, int page, int pageSize)
    {
        var query = _dbContext.Conversations.Where(conversation =>
            conversation.Participants.Any(participant => participant.UserId == userId));
        var total = await query.CountAsync();
        var rows = await query.OrderByDescending(conversation => conversation.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PageResult<ConversationSummaryDto>(rows.Select(conversation =>
            new ConversationSummaryDto(conversation.Id, conversation.CreatedAt,
                conversation.Participants.Select(participant => participant.UserId).ToList(), null)).ToList(), total);
    }

    public async Task<ConversationDto?> GetAsync(Guid conversationId)
    {
        var conversation = await _dbContext.Conversations.FirstOrDefaultAsync(value => value.Id == conversationId);
        return conversation is null ? null : Map(conversation);
    }

    public async Task<MessagePageDto> GetMessagesAsync(
        Guid conversationId, MessageCursorDto? cursor, bool afterCursor, int pageSize)
    {
        var query = _dbContext.ChatMessages.Where(message => message.ConversationId == conversationId);
        if (cursor is not null)
        {
            query = afterCursor
                ? query.Where(message => message.CreatedAt > cursor.CreatedAt ||
                    message.CreatedAt == cursor.CreatedAt && message.Id.CompareTo(cursor.Id) > 0)
                : query.Where(message => message.CreatedAt < cursor.CreatedAt ||
                    message.CreatedAt == cursor.CreatedAt && message.Id.CompareTo(cursor.Id) < 0);
        }
        var rows = afterCursor
            ? await query.OrderBy(message => message.CreatedAt).ThenBy(message => message.Id).Take(pageSize + 1).ToListAsync()
            : await query.OrderByDescending(message => message.CreatedAt).ThenByDescending(message => message.Id).Take(pageSize + 1).ToListAsync();
        var hasMore = rows.Count > pageSize;
        rows = rows.Take(pageSize).ToList();
        if (!afterCursor) rows.Reverse();
        var next = rows.Count == 0 ? null : afterCursor
            ? new MessageCursorDto(rows[^1].CreatedAt, rows[^1].Id)
            : new MessageCursorDto(rows[0].CreatedAt, rows[0].Id);
        return new MessagePageDto(rows.Select(Map).ToList(), next, hasMore);
    }

    private static ConversationDto Map(Conversation value) => new(value.Id, value.Context.Type,
        value.Context.ContextId, value.Participants.Select(participant => new ConversationParticipantDto(
            participant.UserId, null, participant.Role, participant.Mute?.EndsAt,
            participant.Mute?.IsPermanent ?? false)).ToList(), value.CreatedAt);

    private static ChatMessageDto Map(ChatMessage value) => new(value.Id, value.ConversationId,
        value.AuthorId, value.Content?.Value, value.CreatedAt, value.IsDeleted, value.DeletedAt);
}
