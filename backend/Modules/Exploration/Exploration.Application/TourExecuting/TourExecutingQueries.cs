using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecuting;

public sealed class TourExecutingQueries
{
    private readonly ITourExecutionReadRepository _executionReadRepository;
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly TourProgressTracker _progressTracker;

    public TourExecutingQueries(
        ITourExecutionReadRepository executionReadRepository,
        ITourExecutionRepository executionRepository,
        ITourRepository tourRepository,
        TourProgressTracker progressTracker)
    {
        _executionReadRepository = executionReadRepository;
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _progressTracker = progressTracker;
    }

    public async Task<TourExecutionDto?> GetCurrentAsync(Guid touristId, Guid tourId)
    {
        var execution = await _executionRepository.GetLatestAsync(touristId, tourId);
        if (execution is null || execution.Status == TourExecutionStatus.Abandoned)
        {
            return null;
        }
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        var nextKeyPoint = _progressTracker.NextKeyPoint(execution, tour);
        var lastCompleted = execution.CompletedKeyPoints.LastOrDefault();
        var lastReachedKeyPoint = lastCompleted is null
            ? null
            : tour.KeyPoints.Single(keyPoint => keyPoint.Id == lastCompleted.KeyPointId);

        return new TourExecutionDto(
            execution.Id,
            execution.TourId,
            execution.Status,
            nextKeyPoint is null ? null : ToNextKeyPointDto(nextKeyPoint),
            lastReachedKeyPoint is null ? null : ToUnlockedKeyPointDto(lastReachedKeyPoint));
    }

    public Task<List<UnlockedKeyPointDto>> GetUnlockedSecretsAsync(Guid touristId, Guid tourId)
    {
        return _executionReadRepository.GetUnlockedSecretsAsync(touristId, tourId);
    }

    private static NextKeyPointDto ToNextKeyPointDto(KeyPoint keyPoint)
    {
        return new NextKeyPointDto(
            keyPoint.Name,
            keyPoint.Description,
            keyPoint.Image,
            new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude));
    }

    private static UnlockedKeyPointDto ToUnlockedKeyPointDto(KeyPoint keyPoint)
    {
        return new UnlockedKeyPointDto(
            keyPoint.Name,
            new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude),
            keyPoint.Secret);
    }
}
