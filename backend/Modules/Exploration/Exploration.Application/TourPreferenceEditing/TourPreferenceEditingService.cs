using Exploration.Application.TourPreferences;
using Exploration.Domain.TourPreferences;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourPreferenceEditing;

public sealed class TourPreferenceEditingService
{
    private readonly ITourPreferenceRepository _preferenceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TourPreferenceEditingService(ITourPreferenceRepository preferenceRepository, IUnitOfWork unitOfWork)
    {
        _preferenceRepository = preferenceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SaveAsync(Guid touristId, TourPreferenceDto dto)
    {
        var preference = await _preferenceRepository.GetByTouristIdAsync(touristId);
        if (preference is null)
        {
            _preferenceRepository.Add(new TourPreference(touristId, dto.Difficulties, dto.Transport, dto.Tags));
        }
        else
        {
            preference.Update(dto.Difficulties, dto.Transport, dto.Tags);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid touristId)
    {
        var preference = await _preferenceRepository.GetByTouristIdAsync(touristId);
        if (preference is null)
        {
            throw new NotFoundException($"Tourist {touristId} has no tour preference.");
        }

        _preferenceRepository.Remove(preference);
        await _unitOfWork.SaveChangesAsync();
    }
}
