using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Positions;
using Xunit;

namespace Shared.Positions.Tests.Unit.Positions;

public class CoordinatesTests
{
    [Theory]
    [InlineData(90.1, 19.8)]
    [InlineData(-90.1, 19.8)]
    [InlineData(45.2, 180.1)]
    [InlineData(45.2, -180.1)]
    public void Latitude_beyond_90_degrees_or_longitude_beyond_180_degrees_is_rejected(double latitude, double longitude)
    {
        var creation = () => new Coordinates(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }
}
