using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record RecommendedGroupSize
{
    public int Min { get; private init; }
    public int Max { get; private init; }

    private RecommendedGroupSize() { }

    public RecommendedGroupSize(int min, int max)
    {
        if (min < 1)
        {
            throw new DomainException("A recommended group size requires at least one person.");
        }
        if (min > max)
        {
            throw new DomainException("A recommended group size requires the minimum not to exceed the maximum.");
        }

        Min = min;
        Max = max;
    }
}
