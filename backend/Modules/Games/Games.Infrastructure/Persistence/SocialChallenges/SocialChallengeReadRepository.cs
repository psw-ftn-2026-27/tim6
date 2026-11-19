using Games.Application.ChallengeMap;
using Games.Application.Challenges;
using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallenges;
using Games.Domain.Challenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _dbContext;

    public SocialChallengeReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<SocialChallengeDto>> GetAllAsync()
    {
        return _dbContext.SocialChallenges
            .OrderByDescending(challenge => challenge.CreatedAt)
            .ThenBy(challenge => challenge.Id)
            .Select(challenge => new SocialChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Status,
                new DailyTimeWindowDto(challenge.TimeWindow.Start, challenge.TimeWindow.End),
                challenge.RequiredPeople,
                challenge.RangeInMeters,
                challenge.Xp,
                challenge.ExitMessage,
                challenge.Location == null
                    ? null
                    : new LocationDto(challenge.Location.Latitude, challenge.Location.Longitude)))
            .ToListAsync();
    }

    public Task<List<SocialChallengeOnMapDto>> GetPublishedForMapAsync(Guid touristId)
    {
        return _dbContext.SocialChallenges
            .Where(challenge => challenge.Status == ChallengeStatus.Published)
            .Select(challenge => new SocialChallengeOnMapDto(
                challenge.Id,
                challenge.Name,
                challenge.Xp,
                new LocationDto(challenge.Location!.Latitude, challenge.Location.Longitude),
                challenge.RangeInMeters,
                challenge.TimeWindow.Start,
                challenge.TimeWindow.End,
                _dbContext.ChallengeProgresses.Any(progress =>
                    progress.ChallengeId == challenge.Id && progress.TouristId == touristId && progress.SolvedAt != null)))
            .ToListAsync();
    }
}
