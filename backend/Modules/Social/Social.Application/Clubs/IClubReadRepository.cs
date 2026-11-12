using Social.Application.ClubAuthoring;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
}
