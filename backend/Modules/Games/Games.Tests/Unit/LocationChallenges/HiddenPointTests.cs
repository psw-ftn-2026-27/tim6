using FluentAssertions;
using Games.Domain.Challenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class HiddenPointTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Hidden_point_without_an_image_is_rejected(string image)
    {
        var creation = () => new HiddenPoint(image, new Location(45.2553, 19.8449));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Hidden_point_without_a_location_is_rejected()
    {
        var creation = () => new HiddenPoint("/api/shared/images/fasada.jpg", null!);

        creation.Should().Throw<DomainException>();
    }
}
