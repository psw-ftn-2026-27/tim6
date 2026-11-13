using Microsoft.EntityFrameworkCore;
using Social.Application.Following;
using Social.Domain.Following;

namespace Social.Infrastructure.Persistence.Following;

internal sealed class FollowRequestRepository : IFollowRequestRepository
{
    private readonly SocialDbContext _dbContext;

    public FollowRequestRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<FollowRequest?> GetByIdAsync(Guid requestId)
    {
        return _dbContext.FollowRequests.FirstOrDefaultAsync(request => request.Id == requestId);
    }

    public Task<bool> HasPendingAsync(Guid senderId, Guid recipientId)
    {
        return _dbContext.FollowRequests.AnyAsync(request =>
            request.SenderId == senderId
            && request.RecipientId == recipientId
            && request.Status == FollowRequestStatus.Pending);
    }

    public void Add(FollowRequest request)
    {
        _dbContext.FollowRequests.Add(request);
    }

    public void Remove(FollowRequest request)
    {
        _dbContext.FollowRequests.Remove(request);
    }
}
