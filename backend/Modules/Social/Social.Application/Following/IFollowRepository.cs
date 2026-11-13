using Social.Domain.Following;

namespace Social.Application.Following;

public interface IFollowRepository
{
    Task<Follow?> GetAsync(Guid followerId, Guid followedUserId);
    Task<bool> ExistsAsync(Guid followerId, Guid followedUserId);
    void Add(Follow follow);
    void Remove(Follow follow);
}
