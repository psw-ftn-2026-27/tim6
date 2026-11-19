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
        return new MessagePageDto(rows.Select(message => new ChatMessageDto(message.Id,
            message.ConversationId, message.AuthorId, message.Content?.Value, message.CreatedAt,
            message.IsDeleted, message.DeletedAt)).ToList(), next, hasMore);
    }
}
