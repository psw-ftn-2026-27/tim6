using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Following;

public sealed class FollowRequest : AggregateRoot
{
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public FollowRequestStatus Status { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    public bool IsPending => Status == FollowRequestStatus.Pending;

    private FollowRequest() { }

    public FollowRequest(Guid senderId, Guid recipientId) : base(Guid.NewGuid())
    {
        if (senderId == recipientId)
        {
            throw new DomainException("A user cannot send a follow request to themselves.");
        }

        SenderId = senderId;
        RecipientId = recipientId;
        CreatedAt = DateTime.UtcNow;
        Status = FollowRequestStatus.Pending;
    }

    public Follow Accept(Guid actorId)
    {
        EnsureRecipient(actorId);
        Respond(FollowRequestStatus.Accepted);
        return new Follow(SenderId, RecipientId);
    }

    internal Follow AcceptAutomatically()
    {
        Respond(FollowRequestStatus.Accepted);
        return new Follow(SenderId, RecipientId);
    }

    public void Reject(Guid actorId)
    {
        EnsureRecipient(actorId);
        Respond(FollowRequestStatus.Rejected);
    }

    public void EnsureCanBeCancelledBy(Guid actorId)
    {
        if (SenderId != actorId)
        {
            throw NotFoundForActor();
        }

        EnsurePending();
    }

    private void Respond(FollowRequestStatus status)
    {
        EnsurePending();
        Status = status;
        RespondedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (!IsPending)
        {
            throw new DomainException("Only a pending follow request can be acted upon.");
        }
    }

    private void EnsureRecipient(Guid actorId)
    {
        if (RecipientId != actorId)
        {
            throw NotFoundForActor();
        }
    }

    private static NotFoundException NotFoundForActor() =>
        new("The follow request does not exist for this user.");
}
