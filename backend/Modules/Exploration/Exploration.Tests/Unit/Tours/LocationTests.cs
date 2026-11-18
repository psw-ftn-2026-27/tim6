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
}
