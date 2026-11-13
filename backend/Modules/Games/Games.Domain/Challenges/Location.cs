using Shared.Domain.Exceptions;

namespace Games.Domain.Challenges;

public sealed record Location
{
    private const double MaxLatitude = 90;
    private const double MaxLongitude = 180;

    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (latitude < -MaxLatitude || latitude > MaxLatitude)
        {
            throw new DomainException($"A latitude is between {-MaxLatitude} and {MaxLatitude}.");
        }
        if (longitude < -MaxLongitude || longitude > MaxLongitude)
        {
            throw new DomainException($"A longitude is between {-MaxLongitude} and {MaxLongitude}.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
