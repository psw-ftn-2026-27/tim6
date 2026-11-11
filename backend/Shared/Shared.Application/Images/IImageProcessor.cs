using Shared.Domain.Images;

namespace Shared.Application.Images;

public interface IImageProcessor
{
    ImageHeader ReadHeader(Stream content);
    byte[] StripMetadata(Stream content, ImageFormat format);
}
