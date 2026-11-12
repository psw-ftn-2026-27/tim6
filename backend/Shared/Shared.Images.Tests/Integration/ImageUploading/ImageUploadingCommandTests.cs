using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using ImageMagick;
using Shared.Application.ImageUploading;
using Shared.Images.Tests.Integration.Seeds;
using Shared.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Shared.Images.Tests.Integration.ImageUploading;

public class ImageUploadingCommandTests : BaseIntegrationTest
{
    public ImageUploadingCommandTests(SharedImagesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Accepted_image_is_stored_with_the_caller_as_owner_and_its_reference_is_returned()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = FormWith(CreatePicture(MagickFormat.Png), "plaza.png");
        using var arrangeContext = Factory.CreateContext<SharedDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();

        var response = await client.PostAsync("/api/shared/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<ImageReferenceDto>(JsonOptions);
        created!.Reference.Should().StartWith("/api/shared/images/");
        var fileName = created.Reference.Split('/').Last();
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore + 1);
        var stored = assertContext.Images.Single(image => image.FileName == fileName);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        File.Exists(Path.Combine(Factory.StorageDirectory, fileName)).Should().BeTrue();
    }

    [Fact]
    public async Task Stored_image_has_no_exif_metadata()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = FormWith(CreatePhotoWithExif(), "fotografija.jpg");

        var response = await client.PostAsync("/api/shared/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<ImageReferenceDto>(JsonOptions);
        var fileName = created!.Reference.Split('/').Last();
        using var stored = new MagickImage(Path.Combine(Factory.StorageDirectory, fileName));
        stored.GetExifProfile().Should().BeNull();
    }

    [Fact]
    public async Task Text_file_named_as_a_jpeg_is_rejected_and_nothing_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var form = FormWith(Encoding.UTF8.GetBytes("Ovo nije slika, nego tekst."), "slika.jpg");
        using var arrangeContext = Factory.CreateContext<SharedDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();
        var fileCountBefore = Directory.GetFiles(Factory.StorageDirectory).Length;

        var response = await client.PostAsync("/api/shared/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore);
        Directory.GetFiles(Factory.StorageDirectory).Length.Should().Be(fileCountBefore);
    }

    [Fact]
    public async Task Request_larger_than_five_megabytes_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var content = new byte[6 * 1024 * 1024];
        content[0] = 0xFF;
        content[1] = 0xD8;
        content[2] = 0xFF;
        using var form = FormWith(content, "velika.jpg");
        using var arrangeContext = Factory.CreateContext<SharedDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();
        var fileCountBefore = Directory.GetFiles(Factory.StorageDirectory).Length;

        var response = await client.PostAsync("/api/shared/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore);
        Directory.GetFiles(Factory.StorageDirectory).Length.Should().Be(fileCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_upload_an_image()
    {
        var client = Factory.CreateClient();
        using var form = FormWith(CreatePicture(MagickFormat.Png), "plaza.png");

        var response = await client.PostAsync("/api/shared/images", form);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Owner_deletes_their_image_and_it_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var fileName = ImageSeed.ExplorersFortressPhoto.FileName;

        var response = await client.DeleteAsync($"/api/shared/images/{fileName}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.Images.Any(image => image.Id == ImageSeed.ExplorersFortressPhoto.Id).Should().BeFalse();
        File.Exists(Path.Combine(Factory.StorageDirectory, fileName)).Should().BeFalse();
    }

    [Fact]
    public async Task Someone_elses_image_remains_after_a_delete_attempt()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var fileName = ImageSeed.ExplorersFortressPhoto.FileName;

        var response = await client.DeleteAsync($"/api/shared/images/{fileName}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.Images.Any(image => image.Id == ImageSeed.ExplorersFortressPhoto.Id).Should().BeTrue();
        File.Exists(Path.Combine(Factory.StorageDirectory, fileName)).Should().BeTrue();
    }

    [Fact]
    public async Task Deleting_an_image_that_does_not_exist_changes_nothing()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SharedDbContext>();
        var imageCountBefore = arrangeContext.Images.Count();

        var response = await client.DeleteAsync($"/api/shared/images/{Guid.NewGuid()}.png");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.Images.Count().Should().Be(imageCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_an_image()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/shared/images/{ImageSeed.ExplorersFortressPhoto.FileName}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static MultipartFormDataContent FormWith(byte[] content, string fileName)
    {
        var file = new ByteArrayContent(content);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        return new MultipartFormDataContent { { file, "file", fileName } };
    }

    private static byte[] CreatePicture(MagickFormat format)
    {
        using var picture = new MagickImage(MagickColors.Orange, 120, 80);
        picture.Format = format;
        return picture.ToByteArray();
    }

    private static byte[] CreatePhotoWithExif()
    {
        using var photo = new MagickImage(MagickColors.Orange, 120, 80);
        var exif = new ExifProfile();
        exif.SetValue(ExifTag.Make, "Telefon");
        exif.SetValue(ExifTag.GPSLatitudeRef, "N");
        photo.SetProfile(exif);
        photo.Format = MagickFormat.Jpeg;
        return photo.ToByteArray();
    }
}
