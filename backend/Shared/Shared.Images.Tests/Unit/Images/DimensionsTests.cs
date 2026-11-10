using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Xunit;

namespace Shared.Images.Tests.Unit.Images;

public class DimensionsTests
{
    [Theory]
    [InlineData(4097, 600)]
    [InlineData(800, 4097)]
    public void Image_larger_than_4096_by_4096_pixels_is_rejected(int width, int height)
    {
        var creation = () => new Dimensions(width, height);

        creation.Should().Throw<DomainException>();
    }
}
