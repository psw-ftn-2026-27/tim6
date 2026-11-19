namespace Social.Application.ClubMembership;

public sealed record ReceivedClubInvitationDto(Guid ClubId, string ClubName, DateTime SentAt);
