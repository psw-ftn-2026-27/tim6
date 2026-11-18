using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

public sealed record ClubMembershipDto(ClubMembershipStatus Status, ClubRole? Role);
