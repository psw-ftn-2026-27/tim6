using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;
using Shared.Tests;
using Xunit;

namespace Shared.Images.Tests.Unit.Images;

public class ImageTests
{
    private static Image CreateImage(int uploadsInLastHour) =>
        new(WellKnownUsers.Explorer, ImageFormat.Jpeg, new Dimensions(800, 600), 2048, uploadsInLastHour);

    [Fact]
    public void Image_in_a_format_outside_the_whitelist_is_rejected()
    {
        var creation = () => new Image(WellKnownUsers.Explorer, ImageFormat.Unknown, new Dimensions(0, 0), 2048, 0);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Thirtieth_image_of_the_same_user_in_an_hour_is_accepted()
    {
        var creation = () => CreateImage(29);

        creation.Should().NotThrow();
    }

    [Fact]
    public void Thirty_first_image_of_the_same_user_in_an_hour_is_rejected()
    {
        var creation = () => CreateImage(30);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Image_cannot_be_deleted_by_a_user_who_did_not_upload_it()
    {
        var image = CreateImage(0);

        var deletion = () => image.Delete(WellKnownUsers.SecondExplorer);

        deletion.Should().Throw<DomainException>();
    }
}
