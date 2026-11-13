using Social.Domain.Following;

namespace Social.Application.Following;

public interface IFollowRequestRepository
{
    Task<FollowRequest?> GetByIdAsync(Guid requestId);
    Task<bool> HasPendingAsync(Guid senderId, Guid recipientId);
    void Add(FollowRequest request);
    void Remove(FollowRequest request);
}
