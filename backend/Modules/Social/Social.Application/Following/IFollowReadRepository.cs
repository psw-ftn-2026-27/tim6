using Social.Domain.Following;

namespace Social.Application.Following;

public interface IFollowReadRepository
{
    Task<FollowStatus> GetFollowStatusAsync(Guid userId, Guid otherUserId);
    Task<List<SentFollowRequestDto>> GetSentRequestsAsync(Guid senderId);
    Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid followerId);
    Task<List<ReceivedFollowRequestDto>> GetReceivedRequestsAsync(Guid recipientId);
    Task<List<FollowerDto>> GetFollowersAsync(Guid followedUserId);
}
