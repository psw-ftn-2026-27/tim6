using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Following;

public sealed class Follow : AggregateRoot
{
    public Guid FollowerId { get; private set; }
    public Guid FollowedUserId { get; private set; }

    private Follow() { }

    public Follow(Guid followerId, Guid followedUserId) : base(Guid.NewGuid())
    {
        if (followerId == followedUserId)
        {
            throw new DomainException("A user cannot follow themselves.");
        }

        FollowerId = followerId;
        FollowedUserId = followedUserId;
    }
}
