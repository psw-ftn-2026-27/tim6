using FluentAssertions;
using Games.Domain.Challenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Challenges;

public class LocationTests
{
    [Theory]
    [InlineData(-91, 0)]
    [InlineData(91, 0)]
    [InlineData(0, -181)]
    [InlineData(0, 181)]
    public void Location_outside_the_geographic_bounds_is_rejected(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90, 0)]
    [InlineData(90, 0)]
    [InlineData(0, -180)]
    [InlineData(0, 180)]
    public void Location_on_the_geographic_bounds_is_accepted(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Distance_between_two_known_locations_matches_the_expected_distance()
    {
        var equatorAtPrimeMeridian = new Location(0, 0);
        var oneDegreeNorth = new Location(1, 0);

        var distance = equatorAtPrimeMeridian.DistanceTo(oneDegreeNorth);

        distance.Should().BeApproximately(111_195, 1);
    }
}
