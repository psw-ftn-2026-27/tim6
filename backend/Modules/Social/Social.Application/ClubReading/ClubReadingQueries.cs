using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubReading;

public sealed class ClubReadingQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubReadingQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public async Task<ClubDto> GetByIdAsync(Guid id)
    {
        var club = await _clubReadRepository.GetByIdAsync(id);
        if (club is null)
        {
            throw new NotFoundException($"Club {id} does not exist.");
        }

        return club;
    }

    public Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _clubReadRepository.GetPageAsync(page, pageSize);
    }
}
