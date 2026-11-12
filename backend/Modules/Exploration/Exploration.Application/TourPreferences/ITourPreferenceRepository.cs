using Exploration.Domain.TourPreferences;

namespace Exploration.Application.TourPreferences;

public interface ITourPreferenceRepository
{
    Task<TourPreference?> GetByTouristIdAsync(Guid touristId);
    void Add(TourPreference preference);
    void Remove(TourPreference preference);
}
