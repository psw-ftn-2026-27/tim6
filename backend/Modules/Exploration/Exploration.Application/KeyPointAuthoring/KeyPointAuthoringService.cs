using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IUnitOfWork _unitOfWork;

    public KeyPointAuthoringService(ITourRepository tourRepository, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AddKeyPointAsync(Guid tourId, Guid authorId, SaveKeyPointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var location = new Location(dto.Location.Latitude, dto.Location.Longitude);
        var length = new TourLength(dto.LengthInKilometers);

        tour.AddKeyPoint(dto.Name, dto.Description, dto.Image, dto.Secret, location, dto.IsVisibleWithoutPurchase, length);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateKeyPointAsync(Guid tourId, Guid keyPointId, Guid authorId, SaveKeyPointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var location = new Location(dto.Location.Latitude, dto.Location.Longitude);
        var length = new TourLength(dto.LengthInKilometers);

        tour.UpdateKeyPoint(keyPointId, dto.Name, dto.Description, dto.Image, dto.Secret, location,
            dto.IsVisibleWithoutPurchase, length);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveKeyPointAsync(Guid tourId, Guid keyPointId, Guid authorId, RemoveKeyPointDto dto)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var length = new TourLength(dto.LengthInKilometers);

        tour.RemoveKeyPoint(keyPointId, length);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
