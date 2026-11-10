using Shared.Domain.Exceptions;

namespace Shared.Domain.Images;

public sealed class Image : AggregateRoot
{
    private const int MaxUploadsPerHour = 30;
    private static readonly ImageFormat[] SupportedFormats = [ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.WebP];

    public Guid OwnerId { get; private set; }
    public ImageFormat Format { get; private set; }
    public Dimensions Dimensions { get; private set; }
    public long SizeInBytes { get; private set; }
    public DateTime UploadedAt { get; private set; }
    public string FileName { get; private set; }

    private Image()
    {
        Dimensions = null!;
        FileName = null!;
    }

    public Image(Guid ownerId, ImageFormat format, int width, int height, long sizeInBytes, int uploadsInLastHour)
        : base(Guid.NewGuid())
    {
        if (!SupportedFormats.Contains(format))
        {
            throw new DomainException("Only JPEG, PNG and WebP images are accepted.");
        }
        var dimensions = new Dimensions(width, height);
        if (uploadsInLastHour >= MaxUploadsPerHour)
        {
            throw new DomainException($"At most {MaxUploadsPerHour} images may be uploaded in one hour.");
        }

        OwnerId = ownerId;
        Format = format;
        Dimensions = dimensions;
        SizeInBytes = sizeInBytes;
        UploadedAt = DateTime.UtcNow;
        FileName = $"{Id}{ExtensionOf(format)}";
    }

    public void Delete(Guid userId)
    {
        if (userId != OwnerId)
        {
            throw new DomainException("Only the user who uploaded an image may delete it.");
        }
    }

    private static string ExtensionOf(ImageFormat format) => format switch
    {
        ImageFormat.Jpeg => ".jpg",
        ImageFormat.Png => ".png",
        ImageFormat.WebP => ".webp",
        _ => throw new ArgumentOutOfRangeException(nameof(format))
    };
}
