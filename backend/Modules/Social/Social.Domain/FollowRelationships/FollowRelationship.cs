using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Social.Domain.FollowRelationships;

public sealed class FollowRelationship : AggregateRoot
{
    private readonly List<FollowRequest> _requests = [];

    public Guid FirstUserId { get; private set; }
    public Guid SecondUserId { get; private set; }
    public bool FirstFollowsSecond { get; private set; }
    public bool SecondFollowsFirst { get; private set; }
    public IReadOnlyList<FollowRequest> Requests => _requests;

    private FollowRelationship() { }

    public FollowRelationship(Guid userId, Guid otherUserId) : base(Guid.NewGuid())
    {
        if (userId == otherUserId)
        {
            throw new DomainException("A user cannot have a follow relationship with themselves.");
        }

        (FirstUserId, SecondUserId) = OrderPair(userId, otherUserId);
    }

    public static (Guid First, Guid Second) OrderPair(Guid userId, Guid otherUserId) =>
        userId.CompareTo(otherUserId) < 0 ? (userId, otherUserId) : (otherUserId, userId);

    public FollowRequest SendRequest(Guid senderId)
    {
        var recipientId = GetOtherUser(senderId);

        if (Follows(senderId))
        {
            throw new DomainException("The user already follows the other user.");
        }

        if (HasPendingRequest(senderId))
        {
            throw new DomainException("A follow request is already pending.");
        }

        var request = new FollowRequest(senderId, recipientId);
        _requests.Add(request);

        var crossedRequest = _requests.FirstOrDefault(other =>
            other.SenderId == recipientId && other.Status == FollowRequestStatus.Pending);
        if (crossedRequest is not null)
        {
            crossedRequest.Accept();
            SetFollows(recipientId, true);
        }

        if (crossedRequest is not null || Follows(recipientId))
        {
            request.Accept();
            SetFollows(senderId, true);
        }

        return request;
    }

    public void CancelRequest(Guid requestId, Guid actorId)
    {
        EnsureMember(actorId);

        var request = _requests.FirstOrDefault(candidate =>
            candidate.Id == requestId && candidate.SenderId == actorId);
        if (request is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        if (request.Status != FollowRequestStatus.Pending)
        {
            throw new DomainException("Only a pending follow request can be cancelled.");
        }

        _requests.Remove(request);
    }

    public void Unfollow(Guid followerId, Guid actorId)
    {
        EnsureMember(actorId);
        EnsureMember(followerId);

        if (!Follows(followerId))
        {
            throw new DomainException("The user does not follow the other user.");
        }

        SetFollows(followerId, false);
    }

    public FollowStatus GetFollowStatusFor(Guid viewerId)
    {
        EnsureMember(viewerId);

        if (Follows(viewerId))
        {
            return FollowStatus.Following;
        }

        return HasPendingRequest(viewerId) ? FollowStatus.Pending : FollowStatus.Follow;
    }

    private bool Follows(Guid followerId) =>
        followerId == FirstUserId ? FirstFollowsSecond : SecondFollowsFirst;

    private void SetFollows(Guid followerId, bool follows)
    {
        if (followerId == FirstUserId)
        {
            FirstFollowsSecond = follows;
        }
        else
        {
            SecondFollowsFirst = follows;
        }
    }

    private bool HasPendingRequest(Guid senderId) =>
        _requests.Any(request => request.SenderId == senderId && request.Status == FollowRequestStatus.Pending);

    private Guid GetOtherUser(Guid userId)
    {
        EnsureMember(userId);
        return userId == FirstUserId ? SecondUserId : FirstUserId;
    }

    private void EnsureMember(Guid userId)
    {
        if (userId != FirstUserId && userId != SecondUserId)
        {
            throw new NotFoundException("The follow relationship does not exist for this user.");
        }
    }
}
