using Shared.Application.Images;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;

namespace Shared.Application.ImageUploading;

public sealed class ImageUploadingService
{
    private readonly IImageRepository _imageRepository;
    private readonly IImageStorage _imageStorage;
    private readonly IImageProcessor _imageProcessor;
    private readonly IUnitOfWork _unitOfWork;

    public ImageUploadingService(
        IImageRepository imageRepository,
        IImageStorage imageStorage,
        IImageProcessor imageProcessor,
        IUnitOfWork unitOfWork)
    {
        _imageRepository = imageRepository;
        _imageStorage = imageStorage;
        _imageProcessor = imageProcessor;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImageReferenceDto> UploadAsync(Guid userId, Stream content)
    {
        var header = _imageProcessor.ReadHeader(content);
        var uploadsInLastHour = await _imageRepository.CountByOwnerSinceAsync(userId, DateTime.UtcNow.AddHours(-1));
        var image = new Image(userId, header.Format, header.Width, header.Height, content.Length, uploadsInLastHour);

        var stripped = _imageProcessor.StripMetadata(content, image.Format);
        await _imageStorage.SaveAsync(image.FileName, stripped);
        _imageRepository.Add(image);
        await _unitOfWork.SaveChangesAsync();

        return new ImageReferenceDto($"/api/shared/images/{image.FileName}");
    }

    public async Task DeleteAsync(string fileName, Guid userId)
    {
        var image = await _imageRepository.GetByFileNameAsync(fileName);
        if (image is null)
        {
            throw new NotFoundException($"Image {fileName} does not exist.");
        }

        image.Delete(userId);
        _imageRepository.Remove(image);
        await _unitOfWork.SaveChangesAsync();
        _imageStorage.Delete(image.FileName);
    }
}
