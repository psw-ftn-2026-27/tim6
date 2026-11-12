using Shared.Domain;

namespace Social.Domain.FollowRelationships;

public sealed class FollowRequest : Entity
{
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public FollowRequestStatus Status { get; private set; }
    public DateTime? RespondedAt { get; private set; }

    private FollowRequest() { }

    internal FollowRequest(Guid senderId, Guid recipientId) : base(Guid.NewGuid())
    {
        SenderId = senderId;
        RecipientId = recipientId;
        CreatedAt = DateTime.UtcNow;
        Status = FollowRequestStatus.Pending;
    }

    internal void Accept()
    {
        Status = FollowRequestStatus.Accepted;
        RespondedAt = DateTime.UtcNow;
    }

    internal void Reject()
    {
        Status = FollowRequestStatus.Rejected;
        RespondedAt = DateTime.UtcNow;
    }
}
