using Shared.Domain.Exceptions;
using Social.Domain.Following;

namespace Social.Application.Following;

public sealed class FollowingService
{
    private readonly IFollowRepository _followRepository;
    private readonly IFollowRequestRepository _requestRepository;
    private readonly FollowRequestProcessor _requestProcessor;
    private readonly IUnitOfWork _unitOfWork;

    public FollowingService(
        IFollowRepository followRepository,
        IFollowRequestRepository requestRepository,
        FollowRequestProcessor requestProcessor,
        IUnitOfWork unitOfWork)
    {
        _followRepository = followRepository;
        _requestRepository = requestRepository;
        _requestProcessor = requestProcessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SendFollowRequestAsync(Guid userId, Guid recipientId)
    {
        var existingFollow = await _followRepository.GetAsync(userId, recipientId);
        var pendingRequest = await _requestRepository.GetPendingAsync(userId, recipientId);
        var recipientFollowsSender = await _followRepository.GetAsync(recipientId, userId);

        var outcome = _requestProcessor.Send(userId, recipientId, existingFollow, pendingRequest, recipientFollowsSender);
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
        var reversePendingRequest = await _requestRepository.GetPendingAsync(userId, request.SenderId);

        foreach (var follow in _requestProcessor.Accept(request, userId, reversePendingRequest))
        {
            _followRepository.Add(follow);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectFollowRequestAsync(Guid userId, Guid requestId)
    {
        var request = await GetRequestAsync(requestId);

        request.Reject(userId);
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
