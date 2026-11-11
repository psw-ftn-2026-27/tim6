using Social.Application.Clubs;

namespace Social.Application.ClubAuthoring;

public sealed class ClubAuthoringQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubAuthoringQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId)
    {
        return _clubReadRepository.GetByOwnerAsync(ownerId);
    }
}
