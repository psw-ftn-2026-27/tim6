using Shared.Domain.Conversations;

namespace Exploration.Application.Conversations;

public sealed record ConversationParticipantDto(
    Guid UserId, string? UserEmail, ConversationRole Role, DateTime? MutedUntil,
    bool IsPermanentlyMuted);
public sealed record ConversationDto(
    Guid Id, string TourName, string ContextType, Guid? ContextId,
    List<ConversationParticipantDto> Participants, DateTime CreatedAt);
public sealed record ChatMessageDto(
    Guid Id, Guid ConversationId, Guid AuthorId, string? Content, DateTime CreatedAt,
    bool IsDeleted, DateTime? DeletedAt);
public sealed record MessageCursorDto(DateTime CreatedAt, Guid Id);
public sealed record MessagePageDto(List<ChatMessageDto> Items, MessageCursorDto? NextCursor, bool HasMore);
