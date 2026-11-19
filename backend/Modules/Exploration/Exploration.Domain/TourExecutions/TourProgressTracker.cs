using Exploration.Domain.Tours;

namespace Exploration.Domain.TourExecutions;

public sealed class TourProgressTracker
{
    private const double ReachDistanceInMeters = 50;

    public void ReportPosition(TourExecution execution, Tour tour, Location position)
    {
        execution.RecordActivity();

        var nextKeyPoint = NextKeyPoint(execution, tour);
        if (nextKeyPoint is null || nextKeyPoint.Location.DistanceTo(position) > ReachDistanceInMeters)
        {
            return;
        }

        execution.CompleteKeyPoint(nextKeyPoint.Id);
        if (NextKeyPoint(execution, tour) is null)
        {
            execution.Complete();
        }
    }

    public KeyPoint? NextKeyPoint(TourExecution execution, Tour tour)
    {
        return tour.KeyPoints
            .OrderBy(keyPoint => keyPoint.Order)
            .FirstOrDefault(keyPoint => execution.CompletedKeyPoints.All(completed => completed.KeyPointId != keyPoint.Id));
    }
}
