namespace Social.Domain.Clubs;

public sealed record ClubInvitation
{
    public Guid UserId { get; private init; }
    public DateTime SentAt { get; private init; }

    private ClubInvitation() { }

    public ClubInvitation(Guid userId, DateTime sentAt)
    {
        UserId = userId;
        SentAt = sentAt;
    }
}
