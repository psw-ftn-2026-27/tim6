using Microsoft.EntityFrameworkCore;
using Social.Application.FollowRelationships;
using Social.Application.Following;
using Social.Domain.FollowRelationships;

namespace Social.Infrastructure.Persistence.FollowRelationships;

internal sealed class FollowRelationshipReadRepository : IFollowRelationshipReadRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRelationshipReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<SentFollowRequestDto>> GetSentRequestsAsync(Guid senderId)
    {
        return _dbContext.Set<FollowRequest>()
            .Where(request => request.SenderId == senderId)
            .OrderByDescending(request => request.CreatedAt)
            .Select(request => new SentFollowRequestDto(
                request.Id,
                request.RecipientId,
                request.CreatedAt,
                request.Status))
            .ToListAsync();
    }

    public async Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid followerId)
    {
        var followedAsFirstUser = _dbContext.FollowRelationships
            .Where(relationship => relationship.FirstUserId == followerId && relationship.FirstFollowsSecond)
            .Select(relationship => relationship.SecondUserId);

        var followedAsSecondUser = _dbContext.FollowRelationships
            .Where(relationship => relationship.SecondUserId == followerId && relationship.SecondFollowsFirst)
            .Select(relationship => relationship.FirstUserId);

        var followedUserIds = await followedAsFirstUser.Concat(followedAsSecondUser).ToListAsync();

        return followedUserIds.Select(userId => new FollowedUserDto(userId)).ToList();
    }
}
