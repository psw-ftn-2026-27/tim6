using Games.Application.Challenges;
using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;

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
}
