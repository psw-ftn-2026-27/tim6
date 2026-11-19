using Shared.Domain.Conversations;

namespace Social.Application.Conversations;

public sealed record ConversationParticipantDto(
    Guid UserId, string? UserEmail, ConversationRole Role, DateTime? MutedUntil,
    bool IsPermanentlyMuted);

public sealed record ConversationDto(
    Guid Id, string ContextType, Guid? ContextId, List<ConversationParticipantDto> Participants,
    DateTime CreatedAt);

public sealed record ConversationSummaryDto(
    Guid Id, DateTime CreatedAt, List<Guid> ParticipantIds, string? OtherParticipantEmail);

public sealed record ChatMessageDto(
    Guid Id, Guid ConversationId, Guid AuthorId, string? Content, DateTime CreatedAt,
    bool IsDeleted, DateTime? DeletedAt);

public sealed record MessageCursorDto(DateTime CreatedAt, Guid Id);

public sealed record MessagePageDto(List<ChatMessageDto> Items, MessageCursorDto? NextCursor, bool HasMore);

public sealed record ConversationUserDto(Guid UserId, string? Email);

public sealed record UserSearchPageDto(List<ConversationUserDto> Items, int TotalCount);
