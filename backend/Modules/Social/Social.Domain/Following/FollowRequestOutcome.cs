namespace Social.Domain.Following;

public sealed record FollowRequestOutcome(FollowRequest Request, Follow? Follow);
