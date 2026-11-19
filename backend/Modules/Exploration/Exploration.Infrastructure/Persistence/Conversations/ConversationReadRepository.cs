using Exploration.Application.Conversations;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Conversations;

namespace Exploration.Infrastructure.Persistence.Conversations;

internal sealed class ConversationReadRepository : IConversationReadRepository
{
    private readonly ExplorationDbContext _dbContext;
    public ConversationReadRepository(ExplorationDbContext dbContext) => _dbContext = dbContext;

    public async Task<ConversationDto?> GetAsync(Guid conversationId)
    {
        var conversation = await _dbContext.Conversations.FirstOrDefaultAsync(value => value.Id == conversationId);
        return conversation is null ? null : new ConversationDto(conversation.Id, string.Empty,
            conversation.Context.Type, conversation.Context.ContextId,
            conversation.Participants.Select(participant =>
                new ConversationParticipantDto(participant.UserId, null, participant.Role,
                    participant.Mute?.EndsAt, participant.Mute?.IsPermanent ?? false)).ToList(),
            conversation.CreatedAt);
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

        return new MessagePageDto(rows.Select(ToDto).ToList(),
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

    private static ChatMessageDto ToDto(ChatMessage message) => new(
        message.Id, message.ConversationId, message.AuthorId, message.Content?.Value,
        message.CreatedAt, message.IsDeleted, message.DeletedAt);
}
