using Games.Application.LocationChallengeAttempts;
using Games.Domain.LocationChallengeAttempts;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal sealed class LocationChallengeAttemptRepository : ILocationChallengeAttemptRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeAttemptRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LocationChallengeAttempt?> GetByChallengeAndExplorerAsync(Guid challengeId, Guid explorerId)
    {
        return _dbContext.LocationChallengeAttempts.FirstOrDefaultAsync(attempt =>
            attempt.LocationChallengeId == challengeId && attempt.ExplorerId == explorerId);
    }

    public void Add(LocationChallengeAttempt attempt)
    {
        _dbContext.LocationChallengeAttempts.Add(attempt);
    }
}
