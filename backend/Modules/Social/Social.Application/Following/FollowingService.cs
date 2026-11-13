using Shared.Domain.Exceptions;
using Social.Domain.Following;

namespace Social.Application.Following;

public sealed class FollowingService
{
    private readonly IFollowRepository _followRepository;
    private readonly IFollowRequestRepository _requestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowingService(
        IFollowRepository followRepository,
        IFollowRequestRepository requestRepository,
        IUnitOfWork unitOfWork)
    {
        _followRepository = followRepository;
        _requestRepository = requestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SendFollowRequestAsync(Guid userId, Guid recipientId)
    {
        var recipientFollowsSender = await _followRepository.ExistsAsync(recipientId, userId);
        var request = await CreateRequestAsync(userId, recipientId, recipientFollowsSender);
        await _unitOfWork.SaveChangesAsync();

        return request.Id;
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

        AcceptAndStartFollowing(request, userId);
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

        AcceptAndStartFollowing(request, userId);

        var alreadyFollowsSender = await _followRepository.ExistsAsync(userId, request.SenderId);
        var alreadyRequestedSender = await _requestRepository.HasPendingAsync(userId, request.SenderId);
        if (!alreadyFollowsSender && !alreadyRequestedSender)
        {
            await CreateRequestAsync(userId, request.SenderId, recipientFollowsSender: true);
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

    private void AcceptAndStartFollowing(FollowRequest request, Guid recipientId)
    {
        request.Accept(recipientId);
        _followRepository.Add(new Follow(request.SenderId, request.RecipientId));
    }

    private async Task<FollowRequest> CreateRequestAsync(Guid senderId, Guid recipientId, bool recipientFollowsSender)
    {
        var request = new FollowRequest(senderId, recipientId);

        if (await _followRepository.ExistsAsync(senderId, recipientId))
        {
            throw new DomainException("The user already follows the other user.");
        }

        if (await _requestRepository.HasPendingAsync(senderId, recipientId))
        {
            throw new DomainException("A follow request is already pending.");
        }

        _requestRepository.Add(request);

        if (recipientFollowsSender)
        {
            request.AcceptAutomatically();
            _followRepository.Add(new Follow(senderId, recipientId));
        }

        return request;
    }
}
