using Shared.Domain.Exceptions;

namespace Shared.Domain.Positions;

public sealed record Coordinates
{
    private const double MaxLatitude = 90;
    private const double MaxLongitude = 180;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Coordinates() { }

    public Coordinates(double latitude, double longitude)
    {
        if (latitude < -MaxLatitude || latitude > MaxLatitude)
        {
            throw new DomainException($"Latitude must be between -{MaxLatitude} and {MaxLatitude}.");
        }
        if (longitude < -MaxLongitude || longitude > MaxLongitude)
        {
            throw new DomainException($"Longitude must be between -{MaxLongitude} and {MaxLongitude}.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
