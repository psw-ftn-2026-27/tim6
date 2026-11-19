using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

public sealed class TourStarter
{
    public TourExecution Start(Tour tour, TourExecution? activeExecution, bool isPurchased, Guid touristId, Location position)
    {
        if (tour.Status == TourStatus.Draft)
        {
            throw new DomainException("This tour is not available");
        }
        if (!isPurchased)
        {
            throw new DomainException("You haven't purchased this tour");
        }
        if (activeExecution is not null && activeExecution.TourId == tour.Id)
        {
            return activeExecution;
        }
        if (activeExecution is not null)
        {
            throw new DomainException("You already have an active tour. Abandon it first.");
        }

        return new TourExecution(touristId, tour.Id, position);
    }
}
