using Social.Application.Following;

namespace Social.Application.FollowRelationships;

public interface IFollowRelationshipReadRepository
{
    Task<List<SentFollowRequestDto>> GetSentRequestsAsync(Guid senderId);
    Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid followerId);
}
