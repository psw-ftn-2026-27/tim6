using Shared.Domain.Exceptions;

namespace Social.Domain.Following;

public sealed class FollowRequestProcessor
{
    public FollowRequestOutcome Send(
        Guid senderId,
        Guid recipientId,
        Follow? existingFollow,
        FollowRequest? pendingRequest,
        Follow? recipientFollowsSender)
    {
        if (existingFollow is not null)
        {
            throw new DomainException("The user already follows the other user.");
        }

        if (pendingRequest is not null)
        {
            throw new DomainException("A follow request is already pending.");
        }

        var request = new FollowRequest(senderId, recipientId);
        var follow = recipientFollowsSender is null ? null : request.AcceptAutomatically();
        return new FollowRequestOutcome(request, follow);
    }

    public List<Follow> Accept(FollowRequest request, Guid actorId, FollowRequest? reversePendingRequest)
    {
        var follows = new List<Follow> { request.Accept(actorId) };

        if (reversePendingRequest is not null)
        {
            follows.Add(reversePendingRequest.AcceptAutomatically());
        }

        return follows;
    }
}
