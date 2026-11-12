using Games.Application.SocialChallengeAuthoring;
using Games.Application.SocialChallenges;
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
            .Select(challenge => new SocialChallengeDto(
                challenge.Id,
                challenge.Name,
                challenge.Status,
                new DailyTimeWindowDto(challenge.TimeWindow.Start, challenge.TimeWindow.End),
                challenge.RequiredPeople,
                challenge.RangeInMeters,
                challenge.Xp,
                challenge.ExitMessage))
            .ToListAsync();
    }
}
