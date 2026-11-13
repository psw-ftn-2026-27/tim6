using Microsoft.EntityFrameworkCore;
using Social.Application.Following;
using Social.Domain.Following;

namespace Social.Infrastructure.Persistence.Following;

internal sealed class FollowReadRepository : IFollowReadRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<FollowStatus> GetFollowStatusAsync(Guid userId, Guid otherUserId)
    {
        if (await _dbContext.Follows.AnyAsync(follow =>
                follow.FollowerId == userId && follow.FollowedUserId == otherUserId))
        {
            return FollowStatus.Following;
        }

        var hasPendingRequest = await _dbContext.FollowRequests.AnyAsync(request =>
            request.SenderId == userId
            && request.RecipientId == otherUserId
            && request.Status == FollowRequestStatus.Pending);

        return hasPendingRequest ? FollowStatus.Pending : FollowStatus.Follow;
    }

    public Task<List<SentFollowRequestDto>> GetSentRequestsAsync(Guid senderId)
    {
        return _dbContext.FollowRequests
            .Where(request => request.SenderId == senderId)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new SentFollowRequestDto(
                request.Id,
                request.RecipientId,
                request.CreatedAt,
                request.Status))
            .ToListAsync();
    }

    public Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid followerId)
    {
        return _dbContext.Follows
            .Where(follow => follow.FollowerId == followerId)
            .Select(follow => new FollowedUserDto(follow.FollowedUserId))
            .ToListAsync();
    }

    public Task<List<ReceivedFollowRequestDto>> GetReceivedRequestsAsync(Guid recipientId)
    {
        return _dbContext.FollowRequests
            .Where(request => request.RecipientId == recipientId && request.Status == FollowRequestStatus.Pending)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new ReceivedFollowRequestDto(request.Id, request.SenderId, request.CreatedAt))
            .ToListAsync();
    }

    public Task<List<FollowerDto>> GetFollowersAsync(Guid followedUserId)
    {
        return _dbContext.Follows
            .Where(follow => follow.FollowedUserId == followedUserId)
            .Select(follow => new FollowerDto(
                follow.FollowerId,
                _dbContext.Follows.Any(other =>
                    other.FollowerId == followedUserId && other.FollowedUserId == follow.FollowerId),
                _dbContext.FollowRequests.Any(request =>
                    request.SenderId == followedUserId
                    && request.RecipientId == follow.FollowerId
                    && request.Status == FollowRequestStatus.Pending)))
            .ToListAsync();
    }
}
