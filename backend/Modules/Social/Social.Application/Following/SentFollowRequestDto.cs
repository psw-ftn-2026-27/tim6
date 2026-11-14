namespace Social.Application.Following;

public sealed record SentFollowRequestDto(Guid Id, Guid RecipientId, DateTime CreatedAt);
