using Exploration.Application.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public KeyPointAuthoringQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public async Task<TourKeyPointsDto> GetKeyPointsAsync(Guid tourId, Guid authorId)
    {
        var keyPoints = await _tourReadRepository.GetKeyPointsAsync(tourId, authorId);
        if (keyPoints is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return keyPoints;
    }
}
