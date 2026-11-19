using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

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
            throw new DomainException($"A latitude must be between -{MaxLatitude} and {MaxLatitude}.");
        }
        if (longitude < -MaxLongitude || longitude > MaxLongitude)
        {
            throw new DomainException($"A longitude must be between -{MaxLongitude} and {MaxLongitude}.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    public double DistanceTo(Location other)
    {
        var latitudeDifference = ToRadians(other.Latitude - Latitude);
        var longitudeDifference = ToRadians(other.Longitude - Longitude);

        var haversine = Math.Sin(latitudeDifference / 2) * Math.Sin(latitudeDifference / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDifference / 2) * Math.Sin(longitudeDifference / 2);

        return 2 * EarthRadiusInMeters * Math.Asin(Math.Sqrt(haversine));
    }

    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }
}
