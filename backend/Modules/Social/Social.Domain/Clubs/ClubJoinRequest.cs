namespace Social.Domain.Clubs;

public sealed record ClubJoinRequest
{
    public Guid UserId { get; private init; }
    public DateTime SentAt { get; private init; }

    private ClubJoinRequest() { }

    public ClubJoinRequest(Guid userId, DateTime sentAt)
    {
        UserId = userId;
        SentAt = sentAt;
    }
}
