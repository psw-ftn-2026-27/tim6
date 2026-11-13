using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.Following;

/// <summary>
/// A request from <see cref="SenderId"/> to follow <see cref="RecipientId"/>. It is kept after
/// it is answered so the sender can see whether it was accepted or rejected.
/// </summary>
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

    public void Accept(Guid actorId)
    {
        EnsureRecipient(actorId);
        Respond(FollowRequestStatus.Accepted);
    }

    /// <summary>
    /// Accepts the request at the moment it is sent, because the recipient already follows the sender.
    /// </summary>
    public void AcceptAutomatically() => Respond(FollowRequestStatus.Accepted);

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
