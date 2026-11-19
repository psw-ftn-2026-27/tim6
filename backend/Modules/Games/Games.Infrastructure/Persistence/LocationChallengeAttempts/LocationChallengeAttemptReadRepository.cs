using Games.Application.Challenges;
using Games.Application.LocationChallengeAttempts;
using Games.Application.LocationChallengeSolving;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal sealed class LocationChallengeAttemptReadRepository : ILocationChallengeAttemptReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeAttemptReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Only revealed hidden points carry their location; the others stay hidden (P10).
    public Task<LocationChallengeAttemptDto?> GetByChallengeAndExplorerAsync(Guid challengeId, Guid explorerId)
    {
        return _dbContext.LocationChallengeAttempts
            .Where(attempt => attempt.LocationChallengeId == challengeId && attempt.ExplorerId == explorerId)
            .Join(_dbContext.LocationChallenges, attempt => attempt.LocationChallengeId, challenge => challenge.Id,
                (attempt, challenge) => new LocationChallengeAttemptDto(
                    attempt.Id,
                    new LocationDto(challenge.Location!.Latitude, challenge.Location.Longitude),
                    attempt.RadiusInMeters,
                    challenge.HiddenPoints
                        .OrderBy(point => point.Id)
                        .Select(point => new AttemptHiddenPointDto(
                            point.Id,
                            point.Image,
                            attempt.RevealedHiddenPointIds.Contains(point.Id),
                            attempt.RevealedHiddenPointIds.Contains(point.Id)
                                ? new LocationDto(point.Location.Latitude, point.Location.Longitude)
                                : null))
                        .ToList(),
                    attempt.CompletedAt,
                    attempt.EarnedXp))
            .FirstOrDefaultAsync();
    }
}
