using Exploration.Application.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(page, pageSize);
    }

    public async Task<TourPreviewDto> GetPreviewAsync(Guid tourId)
    {
        var preview = await _tourReadRepository.GetPublishedPreviewAsync(tourId);
        if (preview is null)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return preview;
    }
}
