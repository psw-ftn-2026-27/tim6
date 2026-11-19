using Games.Application.ChallengeProgresses;
using Games.Domain.ChallengeProgresses;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.ChallengeProgresses;

internal sealed class ChallengeProgressRepository : IChallengeProgressRepository
{
    private readonly GamesDbContext _dbContext;

    public ChallengeProgressRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ChallengeProgress?> GetAsync(Guid challengeId, Guid touristId)
    {
        return _dbContext.ChallengeProgresses
            .FirstOrDefaultAsync(progress => progress.ChallengeId == challengeId && progress.TouristId == touristId);
    }

    public void Add(ChallengeProgress progress)
    {
        _dbContext.ChallengeProgresses.Add(progress);
    }
}
