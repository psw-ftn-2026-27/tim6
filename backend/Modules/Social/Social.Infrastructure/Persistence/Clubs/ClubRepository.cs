using Microsoft.EntityFrameworkCore;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubRepository : IClubRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Club?> GetByIdAsync(Guid id)
    {
        return _dbContext.Clubs.FirstOrDefaultAsync(club => club.Id == id);
    }

    public void Add(Club club)
    {
        _dbContext.Clubs.Add(club);
    }

    public void Remove(Club club)
    {
        _dbContext.Clubs.Remove(club);
    }
}
