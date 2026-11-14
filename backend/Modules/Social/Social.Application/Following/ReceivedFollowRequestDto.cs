namespace Social.Application.Following;

public sealed record ReceivedFollowRequestDto(Guid Id, Guid SenderId, DateTime CreatedAt);
