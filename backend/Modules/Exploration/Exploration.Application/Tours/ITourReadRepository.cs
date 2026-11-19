using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize);
    Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId);
    Task<TourPreviewDto?> GetPublishedPreviewAsync(Guid tourId);
}
