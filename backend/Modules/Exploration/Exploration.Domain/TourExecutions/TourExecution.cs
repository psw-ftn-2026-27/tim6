using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

public sealed class TourExecution : AggregateRoot
{
    private readonly List<CompletedKeyPoint> _completedKeyPoints = [];

    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public Location StartPosition { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? EndedAt { get; private set; }
    public IReadOnlyList<CompletedKeyPoint> CompletedKeyPoints => _completedKeyPoints;

    private TourExecution()
    {
        StartPosition = null!;
    }

    public TourExecution(Guid touristId, Guid tourId, Location startPosition)
        : base(Guid.NewGuid())
    {
        var now = DateTime.UtcNow;
        TouristId = touristId;
        TourId = tourId;
        StartPosition = startPosition;
        Status = TourExecutionStatus.Active;
        StartedAt = now;
        LastActivityAt = now;
    }

    public void RecordActivity()
    {
        EnsureActive();

        LastActivityAt = DateTime.UtcNow;
    }

    public void CompleteKeyPoint(Guid keyPointId)
    {
        EnsureActive();
        if (_completedKeyPoints.Any(completed => completed.KeyPointId == keyPointId))
        {
            throw new DomainException("This key point is already completed.");
        }

        _completedKeyPoints.Add(new CompletedKeyPoint(keyPointId, DateTime.UtcNow));
    }

    public void Complete()
    {
        EnsureActive();

        Status = TourExecutionStatus.Completed;
        EndedAt = DateTime.UtcNow;
    }

    public void Abandon()
    {
        EnsureActive();

        Status = TourExecutionStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
    }

    private void EnsureActive()
    {
        if (Status != TourExecutionStatus.Active)
        {
            throw new DomainException("This tour is no longer active");
        }
    }
}
