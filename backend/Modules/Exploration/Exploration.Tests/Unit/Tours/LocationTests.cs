using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class LocationTests
{
    [Theory]
    [InlineData(90.1, 19.86)]
    [InlineData(-90.1, 19.86)]
    [InlineData(45.25, 180.1)]
    [InlineData(45.25, -180.1)]
    public void Location_with_a_latitude_or_longitude_out_of_range_is_rejected(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Distance_between_two_known_locations_is_calculated_in_meters()
    {
        var equatorAtPrimeMeridian = new Location(0, 0);
        var oneDegreeEast = new Location(0, 1);

        var distance = equatorAtPrimeMeridian.DistanceTo(oneDegreeEast);

        distance.Should().BeApproximately(111_195, 1);
    }
}
