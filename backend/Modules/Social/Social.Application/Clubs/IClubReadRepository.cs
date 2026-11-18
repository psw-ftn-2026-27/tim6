using Shared.Domain;
using Social.Application.ClubMembership;

namespace Social.Application.Clubs;

public interface IClubReadRepository
{
    Task<ClubDto?> GetByIdAsync(Guid id);
    Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId);
    Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize);
    Task<ClubMembershipDto?> GetMembershipAsync(Guid clubId, Guid userId);
}
