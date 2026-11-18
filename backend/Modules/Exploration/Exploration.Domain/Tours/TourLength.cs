using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourLength
{
    public double Kilometers { get; private init; }

    private TourLength() { }

    public TourLength(double kilometers)
    {
        if (kilometers < 0)
        {
            throw new DomainException("A tour length cannot be negative.");
        }

        Kilometers = kilometers;
    }
}
