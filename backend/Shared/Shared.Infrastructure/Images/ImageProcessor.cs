using ImageMagick;
using Shared.Application.Images;
using Shared.Domain.Images;

namespace Shared.Infrastructure.Images;

internal sealed class ImageProcessor : IImageProcessor
{
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] RiffSignature = [0x52, 0x49, 0x46, 0x46];
    private static readonly byte[] WebPSignature = [0x57, 0x45, 0x42, 0x50];

    public ImageHeader ReadHeader(Stream content)
    {
        var format = DetectFormat(content);
        if (format == ImageFormat.Unknown)
        {
            return new ImageHeader(ImageFormat.Unknown, 0, 0);
        }

        content.Position = 0;
        try
        {
            var info = new MagickImageInfo(content, ReadSettingsFor(format));
            return new ImageHeader(format, ToInt(info.Width), ToInt(info.Height));
        }
        catch (MagickException)
        {
            return new ImageHeader(ImageFormat.Unknown, 0, 0);
        }
    }

    public byte[] StripMetadata(Stream content, ImageFormat format)
    {
        content.Position = 0;
        using var image = new MagickImage(content, ReadSettingsFor(format));
        image.AutoOrient();
        image.Strip();
        return image.ToByteArray();
    }

    private static ImageFormat DetectFormat(Stream content)
    {
        var leading = new byte[12];
        content.Position = 0;
        content.ReadAtLeast(leading, leading.Length, throwOnEndOfStream: false);

        if (leading.AsSpan().StartsWith(JpegSignature))
        {
            return ImageFormat.Jpeg;
        }
        if (leading.AsSpan().StartsWith(PngSignature))
        {
            return ImageFormat.Png;
        }
        if (leading.AsSpan(0, 4).SequenceEqual(RiffSignature) && leading.AsSpan(8, 4).SequenceEqual(WebPSignature))
        {
            return ImageFormat.WebP;
        }
        return ImageFormat.Unknown;
    }

    private static MagickReadSettings ReadSettingsFor(ImageFormat format)
    {
        return new MagickReadSettings
        {
            Format = format switch
            {
                ImageFormat.Jpeg => MagickFormat.Jpeg,
                ImageFormat.Png => MagickFormat.Png,
                ImageFormat.WebP => MagickFormat.WebP,
                _ => throw new ArgumentOutOfRangeException(nameof(format))
            }
        };
    }

    private static int ToInt(uint pixels)
    {
        return (int)Math.Min(pixels, int.MaxValue);
    }
}
