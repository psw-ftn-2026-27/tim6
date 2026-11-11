using Microsoft.EntityFrameworkCore;
using Social.Application.FollowRelationships;
using Social.Domain.FollowRelationships;

namespace Social.Infrastructure.Persistence.FollowRelationships;

internal sealed class FollowRelationshipRepository : IFollowRelationshipRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRelationshipRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FollowRelationship?> GetByPairAsync(Guid userId, Guid otherUserId)
    {
        var (firstUserId, secondUserId) = FollowRelationship.OrderPair(userId, otherUserId);

        return _dbContext.FollowRelationships.FirstOrDefaultAsync(relationship =>
            relationship.FirstUserId == firstUserId && relationship.SecondUserId == secondUserId);
    }

    public Task<FollowRelationship?> GetByRequestIdAsync(Guid requestId)
    {
        return _dbContext.FollowRelationships.FirstOrDefaultAsync(relationship =>
            relationship.Requests.Any(request => request.Id == requestId));
    }

    public void Add(FollowRelationship relationship)
    {
        _dbContext.FollowRelationships.Add(relationship);
    }
}
