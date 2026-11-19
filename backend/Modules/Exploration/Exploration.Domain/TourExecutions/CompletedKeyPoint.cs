namespace Exploration.Domain.TourExecutions;

public sealed record CompletedKeyPoint
{
    public Guid KeyPointId { get; private init; }
    public DateTime ReachedAt { get; private init; }

    private CompletedKeyPoint() { }

    public CompletedKeyPoint(Guid keyPointId, DateTime reachedAt)
    {
        KeyPointId = keyPointId;
        ReachedAt = reachedAt;
    }
}
