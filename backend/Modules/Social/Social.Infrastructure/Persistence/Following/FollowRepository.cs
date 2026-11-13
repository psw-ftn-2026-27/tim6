using Microsoft.EntityFrameworkCore;
using Social.Application.Following;
using Social.Domain.Following;

namespace Social.Infrastructure.Persistence.Following;

internal sealed class FollowRepository : IFollowRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Follow?> GetAsync(Guid followerId, Guid followedUserId)
    {
        return _dbContext.Follows.FirstOrDefaultAsync(follow =>
            follow.FollowerId == followerId && follow.FollowedUserId == followedUserId);
    }

    public Task<bool> ExistsAsync(Guid followerId, Guid followedUserId)
    {
        return _dbContext.Follows.AnyAsync(follow =>
            follow.FollowerId == followerId && follow.FollowedUserId == followedUserId);
    }

    public void Add(Follow follow)
    {
        _dbContext.Follows.Add(follow);
    }

    public void Remove(Follow follow)
    {
        _dbContext.Follows.Remove(follow);
    }
}
