using Shared.Application.Images;
using Shared.Domain.Exceptions;

namespace Shared.Application.ImageReading;

public sealed class ImageReadingQueries
{
    private static readonly Dictionary<string, string> ContentTypes = new()
    {
        [".jpg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp"
    };

    private readonly IImageStorage _imageStorage;

    public ImageReadingQueries(IImageStorage imageStorage)
    {
        _imageStorage = imageStorage;
    }

    public async Task<ImageFileDto> GetAsync(string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (!ContentTypes.TryGetValue(Path.GetExtension(name), out var contentType))
        {
            throw new NotFoundException($"Image {fileName} does not exist.");
        }

        var content = await _imageStorage.ReadAsync(name);
        if (content is null)
        {
            throw new NotFoundException($"Image {fileName} does not exist.");
        }

        return new ImageFileDto(content, contentType);
    }
}
