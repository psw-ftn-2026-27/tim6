using Exploration.Application.TourPreferenceEditing;
using Exploration.Application.TourPreferences;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourPreferences;

internal sealed class TourPreferenceReadRepository : ITourPreferenceReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourPreferenceReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TourPreferenceDto?> GetByTouristIdAsync(Guid touristId)
    {
        return _dbContext.TourPreferences
            .Where(preference => preference.TouristId == touristId)
            .Select(preference => new TourPreferenceDto(preference.Difficulties, preference.Transport, preference.Tags, preference.ApplyToSearch))
            .FirstOrDefaultAsync();
    }
}
