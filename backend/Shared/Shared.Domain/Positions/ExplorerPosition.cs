namespace Shared.Domain.Positions;

public sealed class ExplorerPosition : AggregateRoot
{
    public Guid ExplorerId { get; private set; }
    public Coordinates Coordinates { get; private set; }

    private ExplorerPosition()
    {
        Coordinates = null!;
    }

    public ExplorerPosition(Guid explorerId, Coordinates coordinates)
        : base(Guid.NewGuid())
    {
        ExplorerId = explorerId;
        Coordinates = coordinates;
    }

    public void MoveTo(Coordinates coordinates)
    {
        Coordinates = coordinates;
    }
}
