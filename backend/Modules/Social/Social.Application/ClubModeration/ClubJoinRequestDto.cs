namespace Social.Application.ClubModeration;

public sealed record ClubJoinRequestDto(Guid UserId, string? UserEmail, DateTime SentAt);
