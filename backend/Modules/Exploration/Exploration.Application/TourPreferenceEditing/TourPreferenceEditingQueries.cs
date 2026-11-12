using Exploration.Application.TourPreferences;

namespace Exploration.Application.TourPreferenceEditing;

public sealed class TourPreferenceEditingQueries
{
    private readonly ITourPreferenceReadRepository _preferenceReadRepository;

    public TourPreferenceEditingQueries(ITourPreferenceReadRepository preferenceReadRepository)
    {
        _preferenceReadRepository = preferenceReadRepository;
    }

    public Task<TourPreferenceDto?> GetMineAsync(Guid touristId)
    {
        return _preferenceReadRepository.GetByTouristIdAsync(touristId);
    }
}
