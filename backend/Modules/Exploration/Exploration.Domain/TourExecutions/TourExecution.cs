using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

public sealed class TourExecution : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Guid TourId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public Location StartPosition { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? EndedAt { get; private set; }

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

    public void Abandon()
    {
        if (Status != TourExecutionStatus.Active)
        {
            throw new DomainException("This tour is no longer active");
        }

        Status = TourExecutionStatus.Abandoned;
        EndedAt = DateTime.UtcNow;
    }
}
