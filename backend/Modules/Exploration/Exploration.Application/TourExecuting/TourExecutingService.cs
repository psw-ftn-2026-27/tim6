using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecuting;

public sealed class TourExecutingService
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IPaymentApi _paymentApi;
    private readonly TourStarter _tourStarter;
    private readonly TourProgressTracker _progressTracker;
    private readonly IUnitOfWork _unitOfWork;

    public TourExecutingService(
        ITourExecutionRepository executionRepository,
        ITourRepository tourRepository,
        IPaymentApi paymentApi,
        TourStarter tourStarter,
        TourProgressTracker progressTracker,
        IUnitOfWork unitOfWork)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _paymentApi = paymentApi;
        _tourStarter = tourStarter;
        _progressTracker = progressTracker;
        _unitOfWork = unitOfWork;
    }

    public async Task StartAsync(Guid touristId, StartTourDto dto)
    {
        var isPurchased = await _paymentApi.HasPurchasedAsync(touristId, dto.TourId);
        var tour = await _tourRepository.GetByIdAsync(dto.TourId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {dto.TourId} does not exist.");
        }
        var activeExecution = await _executionRepository.GetActiveByTouristIdAsync(touristId);
        var position = new Location(dto.Position.Latitude, dto.Position.Longitude);

        var execution = _tourStarter.Start(tour, activeExecution, isPurchased, touristId, position);
        if (execution != activeExecution)
        {
            _executionRepository.Add(execution);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AbandonAsync(Guid touristId, Guid tourId)
    {
        var execution = await _executionRepository.GetActiveByTouristIdAsync(touristId);
        if (execution is null || execution.TourId != tourId)
        {
            throw new NotFoundException("This tour is no longer active");
        }

        execution.Abandon();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReportPositionAsync(Guid touristId, Guid tourId, LocationDto position)
    {
        var execution = await _executionRepository.GetActiveByTouristIdAsync(touristId);
        if (execution is null || execution.TourId != tourId)
        {
            throw new NotFoundException("This tour is no longer active");
        }
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }

        _progressTracker.ReportPosition(execution, tour, new Location(position.Latitude, position.Longitude));
        await _unitOfWork.SaveChangesAsync();
    }
}
