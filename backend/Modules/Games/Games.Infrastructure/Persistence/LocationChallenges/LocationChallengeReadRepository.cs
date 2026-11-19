using Games.Application.Challenges;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallengeSolving;
using Games.Application.LocationChallenges;
using Games.Domain.Challenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public LocationChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<LocationChallengeDto>> GetAllAsync()
    {
        return _dbContext.LocationChallenges
            .OrderByDescending(challenge => challenge.CreatedAt)
            .ThenBy(challenge => challenge.Id)
            .Select(challenge => new LocationChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Xp,
                challenge.Status,
                challenge.HiddenPoints
                    .OrderBy(point => point.Id)
                    .Select(point => new HiddenPointDto(
                        point.Image,
                        new LocationDto(point.Location.Latitude, point.Location.Longitude)))
                    .ToList(),
                challenge.Location == null
                    ? null
                    : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude)))
            .ToListAsync();
    }

    // Published challenges with the explorer's state on each, without hidden point locations (P10).
    // A solved challenge stays in the list; its state comes from the explorer's progress, as for quizzes.
    public async Task<PageResult<AvailableLocationChallengeDto>> GetAvailableAsync(Guid explorerId, int page, int pageSize)
    {
        var available = _dbContext.LocationChallenges
            .Where(challenge => challenge.Status == ChallengeStatus.Published)
            .OrderByDescending(challenge => challenge.CreatedAt)
            .ThenBy(challenge => challenge.Id);

        var totalCount = await available.CountAsync();
        var items = await available
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(challenge => new AvailableLocationChallengeDto(
                challenge.Id,
                challenge.Name,
                new LocationDto(challenge.Location!.Latitude, challenge.Location.Longitude),
                challenge.Xp,
                _dbContext.LocationChallengeAttempts.Any(attempt =>
                    attempt.LocationChallengeId == challenge.Id
                    && attempt.ExplorerId == explorerId
                    && attempt.CompletedAt == null),
                _dbContext.ChallengeProgresses.Any(progress =>
                    progress.ChallengeId == challenge.Id && progress.TouristId == explorerId && progress.SolvedAt != null)))
            .ToListAsync();

        return new PageResult<AvailableLocationChallengeDto>(items, totalCount);
    }
}
