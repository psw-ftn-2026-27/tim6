namespace Social.Application.ClubInvitations;

public sealed record ClubInvitationDto(Guid UserId, string? UserEmail, DateTime SentAt);
