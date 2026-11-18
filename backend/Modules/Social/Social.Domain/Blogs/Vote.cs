using Shared.Domain;

namespace Social.Domain.Blogs;

public sealed class Vote : Entity
{
    public Guid UserId { get; private set; }
    public VoteSign Sign { get; private set; }
    public DateTime VotedAt { get; private set; }

    private Vote() { }

    public Vote(Guid userId, VoteSign sign, DateTime now) : base(Guid.NewGuid())
    {
        UserId = userId;
        Sign = sign;
        VotedAt = now;
    }

    public void ChangeSign(VoteSign sign, DateTime now)
    {
        Sign = sign;
        VotedAt = now;
    }
}
