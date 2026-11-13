using Shared.Domain.Exceptions;
using Social.Domain.Following;

namespace Social.Application.Following;

public sealed class FollowingService
{
    private readonly IFollowRepository _followRepository;
    private readonly IFollowRequestRepository _requestRepository;
    private readonly FollowRequestSender _requestSender;
    private readonly IUnitOfWork _unitOfWork;

    public FollowingService(
        IFollowRepository followRepository,
        IFollowRequestRepository requestRepository,
        FollowRequestSender requestSender,
        IUnitOfWork unitOfWork)
    {
        _followRepository = followRepository;
        _requestRepository = requestRepository;
        _requestSender = requestSender;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SendFollowRequestAsync(Guid userId, Guid recipientId)
    {
        var existingFollow = await _followRepository.GetAsync(userId, recipientId);
        var pendingRequest = await _requestRepository.GetPendingAsync(userId, recipientId);
        var recipientFollowsSender = await _followRepository.GetAsync(recipientId, userId);

        var outcome = _requestSender.Send(userId, recipientId, existingFollow, pendingRequest, recipientFollowsSender);
        AddOutcome(outcome);
        await _unitOfWork.SaveChangesAsync();

        return outcome.Request.Id;
    }

    public async Task CancelFollowRequestAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);

        request.EnsureCanBeCancelledBy(userId);
        _requestRepository.Remove(request);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnfollowUserAsync(Guid userId, Guid followedUserId)
    {
        var follow = await _followRepository.GetAsync(userId, followedUserId)
            ?? throw new NotFoundException($"User {followedUserId} is not followed.");

        _followRepository.Remove(follow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptFollowRequestAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);

        _followRepository.Add(request.Accept(userId));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectFollowRequestAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);

        request.Reject(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptAndFollowBackAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);
        var existingFollowBack = await _followRepository.GetAsync(userId, request.SenderId);
        var pendingFollowBack = await _requestRepository.GetPendingAsync(userId, request.SenderId);

        var follow = request.Accept(userId);
        _followRepository.Add(follow);

        var followBack = _requestSender.FollowBack(follow, existingFollowBack, pendingFollowBack);
        if (followBack is not null)
        {
            AddOutcome(followBack);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFollowerAsync(Guid userId, Guid followerId)
    {
        var follow = await _followRepository.GetAsync(followerId, userId)
            ?? throw new NotFoundException($"User {followerId} does not follow you.");

        _followRepository.Remove(follow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<FollowRequest> GetRequestAsync(Guid requestId)
    {
        return await _requestRepository.GetByIdAsync(requestId)
            ?? throw new NotFoundException($"Follow request {requestId} does not exist.");
    }

    private void AddOutcome(FollowRequestOutcome outcome)
    {
        _requestRepository.Add(outcome.Request);
        if (outcome.Follow is not null)
        {
            _followRepository.Add(outcome.Follow);
        }
    }
}
