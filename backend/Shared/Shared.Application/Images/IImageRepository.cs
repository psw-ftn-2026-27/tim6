using Shared.Domain.Images;

namespace Shared.Application.Images;

public interface IImageRepository
{
    Task<Image?> GetByFileNameAsync(string fileName);
    Task<int> CountByOwnerSinceAsync(Guid ownerId, DateTime since);
    void Add(Image image);
    void Remove(Image image);
}
