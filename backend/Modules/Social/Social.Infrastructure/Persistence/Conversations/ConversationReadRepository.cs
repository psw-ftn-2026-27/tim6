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
        query = ApplyCursor(query, cursor, afterCursor);
        var rows = await LoadPageAsync(query, afterCursor, pageSize);
        var hasMore = rows.Count > pageSize;
        rows = rows.Take(pageSize).ToList();
        if (!afterCursor)
        {
            rows.Reverse();
        }

        return new MessagePageDto(rows.Select(Map).ToList(),
            CreateNextCursor(rows, afterCursor), hasMore);
    }

    private static IQueryable<ChatMessage> ApplyCursor(
        IQueryable<ChatMessage> query, MessageCursorDto? cursor, bool afterCursor)
    {
        if (cursor is null)
        {
            return query;
        }

        return afterCursor
            ? query.Where(message => message.CreatedAt > cursor.CreatedAt ||
                message.CreatedAt == cursor.CreatedAt && message.Id.CompareTo(cursor.Id) > 0)
            : query.Where(message => message.CreatedAt < cursor.CreatedAt ||
                message.CreatedAt == cursor.CreatedAt && message.Id.CompareTo(cursor.Id) < 0);
    }

    private static async Task<List<ChatMessage>> LoadPageAsync(
        IQueryable<ChatMessage> query, bool afterCursor, int pageSize) =>
        afterCursor
            ? await query.OrderBy(message => message.CreatedAt)
                .ThenBy(message => message.Id).Take(pageSize + 1).ToListAsync()
            : await query.OrderByDescending(message => message.CreatedAt)
                .ThenByDescending(message => message.Id).Take(pageSize + 1).ToListAsync();

    private static MessageCursorDto? CreateNextCursor(
        IReadOnlyList<ChatMessage> rows, bool afterCursor)
    {
        if (rows.Count == 0)
        {
            return null;
        }

        var boundary = afterCursor ? rows[^1] : rows[0];
        return new MessageCursorDto(boundary.CreatedAt, boundary.Id);
    }

    private static ConversationDto Map(Conversation value) => new(value.Id, value.Context.Type,
        value.Context.ContextId, value.Participants.Select(participant => new ConversationParticipantDto(
            participant.UserId, null, participant.Role, participant.Mute?.EndsAt,
            participant.Mute?.IsPermanent ?? false)).ToList(), value.CreatedAt);

    private static ChatMessageDto Map(ChatMessage value) => new(value.Id, value.ConversationId,
        value.AuthorId, value.Content?.Value, value.CreatedAt, value.IsDeleted, value.DeletedAt);
}
