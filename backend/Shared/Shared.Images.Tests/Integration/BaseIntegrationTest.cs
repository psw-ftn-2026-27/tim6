using System.Text.Json;
using System.Text.Json.Serialization;
using ImageMagick;
using Microsoft.AspNetCore.Hosting;
using Shared.Domain.Images;
using Shared.Images.Tests.Integration.Seeds;
using Shared.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Shared.Images.Tests.Integration;

public sealed class SharedImagesApiFactory : ExplorerApiFactory
{
    public string StorageDirectory { get; } = Path.Combine(Path.GetTempPath(), $"explorer-images-{Guid.NewGuid()}");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.UseSetting("ImageStorage:Directory", StorageDirectory);
    }

    /// <summary>
    /// Restores the image storage directory to the seeded baseline: removes every stored
    /// file and writes a generated picture for each seeded image under its file name.
    /// </summary>
    public void ReseedStorage(IEnumerable<Image> images)
    {
        if (Directory.Exists(StorageDirectory))
        {
            Directory.Delete(StorageDirectory, recursive: true);
        }
        Directory.CreateDirectory(StorageDirectory);

        foreach (var image in images)
        {
            using var picture = new MagickImage(MagickColors.SteelBlue, (uint)image.Dimensions.Width, (uint)image.Dimensions.Height);
            picture.Write(Path.Combine(StorageDirectory, image.FileName));
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (Directory.Exists(StorageDirectory))
        {
            Directory.Delete(StorageDirectory, recursive: true);
        }
    }
}

[CollectionDefinition("Integration")]
public sealed class IntegrationCollection : ICollectionFixture<SharedImagesApiFactory>;

[Collection("Integration")]
public abstract class BaseIntegrationTest
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected readonly SharedImagesApiFactory Factory;

    protected BaseIntegrationTest(SharedImagesApiFactory factory)
    {
        Factory = factory;
        Factory.Reseed<SharedDbContext>(SharedSeed.All);
        Factory.ReseedStorage(ImageSeed.All);
    }
}
