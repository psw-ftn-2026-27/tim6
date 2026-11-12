using Shared.Domain.Exceptions;
using Social.Application.FollowRelationships;
using Social.Domain.FollowRelationships;

namespace Social.Application.Following;

public sealed class FollowingService
{
    private readonly IFollowRelationshipRepository _relationshipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FollowingService(IFollowRelationshipRepository relationshipRepository, IUnitOfWork unitOfWork)
    {
        _relationshipRepository = relationshipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SendFollowRequestAsync(Guid userId, Guid recipientId)
    {
        var relationship = await _relationshipRepository.GetByPairAsync(userId, recipientId);
        if (relationship is null)
        {
            relationship = new FollowRelationship(userId, recipientId);
            _relationshipRepository.Add(relationship);
        }

        var request = relationship.SendRequest(userId);
        await _unitOfWork.SaveChangesAsync();

        return request.Id;
    }

    public async Task CancelFollowRequestAsync(Guid userId, Guid requestId)
    {
        var relationship = await _relationshipRepository.GetByRequestIdAsync(requestId);
        if (relationship is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        relationship.CancelRequest(requestId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UnfollowUserAsync(Guid userId, Guid followedUserId)
    {
        var relationship = await _relationshipRepository.GetByPairAsync(userId, followedUserId);
        if (relationship is null)
        {
            throw new NotFoundException($"User {followedUserId} is not followed.");
        }

        relationship.Unfollow(userId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptFollowRequestAsync(Guid userId, Guid requestId)
    {
        var relationship = await _relationshipRepository.GetByRequestIdAsync(requestId);
        if (relationship is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        relationship.AcceptRequest(requestId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectFollowRequestAsync(Guid userId, Guid requestId)
    {
        var relationship = await _relationshipRepository.GetByRequestIdAsync(requestId);
        if (relationship is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        relationship.RejectRequest(requestId, userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AcceptAndFollowBackAsync(Guid userId, Guid requestId)
    {
        var relationship = await _relationshipRepository.GetByRequestIdAsync(requestId);
        if (relationship is null)
        {
            throw new NotFoundException($"Follow request {requestId} does not exist.");
        }

        relationship.AcceptRequest(requestId, userId);
        relationship.SendRequest(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveFollowerAsync(Guid userId, Guid followerId)
    {
        var relationship = await _relationshipRepository.GetByPairAsync(userId, followerId);
        if (relationship is null)
        {
            throw new NotFoundException($"User {followerId} does not follow you.");
        }

        relationship.Unfollow(followerId, userId);
        await _unitOfWork.SaveChangesAsync();
    }
}
