using System.Net;
using FluentAssertions;
using ImageMagick;
using Shared.Images.Tests.Integration.Seeds;
using Xunit;

namespace Shared.Images.Tests.Integration.ImageReading;

public class ImageReadingQueryTests : BaseIntegrationTest
{
    public ImageReadingQueryTests(SharedImagesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Image_is_retrieved_by_its_reference_without_signing_in()
    {
        var client = Factory.CreateClient();
        var seeded = ImageSeed.ExplorersFortressPhoto;

        var response = await client.GetAsync($"/api/shared/images/{seeded.FileName}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("image/png");
        using var picture = new MagickImage(await response.Content.ReadAsByteArrayAsync());
        picture.Width.Should().Be((uint)seeded.Dimensions.Width);
        picture.Height.Should().Be((uint)seeded.Dimensions.Height);
    }
}
