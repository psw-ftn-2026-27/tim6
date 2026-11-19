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

    // Great-circle distance in meters, by the haversine formula.
    public double DistanceTo(Location other)
    {
        var latitude = ToRadians(Latitude);
        var otherLatitude = ToRadians(other.Latitude);
        var latitudeDelta = otherLatitude - latitude;
        var longitudeDelta = ToRadians(other.Longitude - Longitude);

        var haversine = Math.Pow(Math.Sin(latitudeDelta / 2), 2)
            + Math.Cos(latitude) * Math.Cos(otherLatitude) * Math.Pow(Math.Sin(longitudeDelta / 2), 2);

        return 2 * EarthRadiusInMeters * Math.Asin(Math.Sqrt(haversine));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
