using Microsoft.EntityFrameworkCore;
using Shared.Application.Images;
using Shared.Domain.Images;

namespace Shared.Infrastructure.Persistence.Images;

internal sealed class ImageRepository : IImageRepository
{
    private readonly SharedDbContext _dbContext;

    public ImageRepository(SharedDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Image?> GetByFileNameAsync(string fileName)
    {
        return _dbContext.Images.FirstOrDefaultAsync(image => image.FileName == fileName);
    }

    public Task<int> CountByOwnerSinceAsync(Guid ownerId, DateTime since)
    {
        return _dbContext.Images.CountAsync(image => image.OwnerId == ownerId && image.UploadedAt >= since);
    }

    public void Add(Image image)
    {
        _dbContext.Images.Add(image);
    }

    public void Remove(Image image)
    {
        _dbContext.Images.Remove(image);
    }
}
