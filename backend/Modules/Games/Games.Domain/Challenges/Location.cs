using Shared.Domain.Exceptions;

namespace Games.Domain.Challenges;

public sealed record Location
{
    private const double MaxLatitude = 90;
    private const double MaxLongitude = 180;
    private const double EarthRadiusInMeters = 6_371_000;

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

    // The great-circle distance by the haversine formula, treating the Earth as a sphere.
    public double DistanceTo(Location other)
    {
        var latitudeDelta = ToRadians(other.Latitude - Latitude);
        var longitudeDelta = ToRadians(other.Longitude - Longitude);

        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);

        return EarthRadiusInMeters * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
