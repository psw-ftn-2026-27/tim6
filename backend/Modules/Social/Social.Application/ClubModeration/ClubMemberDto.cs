using Social.Domain.Clubs;

namespace Social.Application.ClubModeration;

public sealed record ClubMemberDto(Guid UserId, string? UserEmail, ClubRole Role);
