using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
    {
        var published = _dbContext.Tours
            .Where(tour => tour.Status == TourStatus.Published)
            .OrderByDescending(tour => tour.PublishedAt);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(published)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    public Task<TourKeyPointsDto?> GetKeyPointsAsync(Guid tourId, Guid authorId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.AuthorId == authorId)
            .Select(tour => new TourKeyPointsDto(
                tour.Status,
                tour.KeyPoints
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new KeyPointDto(
                        keyPoint.Id,
                        keyPoint.Order,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.Image,
                        keyPoint.Secret,
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude),
                        keyPoint.IsVisibleWithoutPurchase))
                    .ToList(),
                tour.Length.Kilometers))
            .FirstOrDefaultAsync();
    }

    public Task<TourPreviewDto?> GetPublishedPreviewAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.Status == TourStatus.Published)
            .Select(tour => new TourPreviewDto(
                tour.Id,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                new RecommendedGroupSizeDto(tour.RecommendedGroupSize.Min, tour.RecommendedGroupSize.Max),
                tour.Length.Kilometers,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.KeyPoints
                    .Where(keyPoint => keyPoint.IsVisibleWithoutPurchase)
                    .OrderBy(keyPoint => keyPoint.Order)
                    .Select(keyPoint => new KeyPointPreviewDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.Image,
                        new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude)))
                    .ToList()))
            .FirstOrDefaultAsync();
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.ArchivedAt,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                new RecommendedGroupSizeDto(tour.RecommendedGroupSize.Min, tour.RecommendedGroupSize.Max)));
    }
}
