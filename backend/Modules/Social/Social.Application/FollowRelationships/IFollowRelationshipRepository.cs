using Social.Domain.FollowRelationships;

namespace Social.Application.FollowRelationships;

public interface IFollowRelationshipRepository
{
    Task<FollowRelationship?> GetByPairAsync(Guid userId, Guid otherUserId);
    Task<FollowRelationship?> GetByRequestIdAsync(Guid requestId);
    void Add(FollowRelationship relationship);
}
