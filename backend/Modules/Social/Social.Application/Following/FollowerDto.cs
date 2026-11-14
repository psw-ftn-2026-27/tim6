namespace Social.Application.Following;

public sealed record FollowerDto(Guid UserId, bool IsFollowedBack);
