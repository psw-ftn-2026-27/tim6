using Shared.Domain.Exceptions;

namespace Social.Domain.Following;

public sealed class FollowRequestSender
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

    public FollowRequestOutcome? FollowBack(
        Follow acceptedFollow,
        Follow? existingFollowBack,
        FollowRequest? pendingFollowBack)
    {
        if (existingFollowBack is not null || pendingFollowBack is not null)
        {
            return null;
        }

        var request = new FollowRequest(acceptedFollow.FollowedUserId, acceptedFollow.FollowerId);
        return new FollowRequestOutcome(request, request.AcceptAutomatically());
    }
}
