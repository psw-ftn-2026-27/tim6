using Microsoft.EntityFrameworkCore;
using Shared.Domain;
using Social.Application.ClubMembership;
using Social.Application.ClubModeration;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal sealed class ClubReadRepository : IClubReadRepository
{
    private readonly SocialDbContext _dbContext;

    public ClubReadRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ClubDto?> GetByIdAsync(Guid id)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.Id == id)).FirstOrDefaultAsync();
    }

    public Task<List<ClubDto>> GetByOwnerAsync(Guid ownerId)
    {
        return ProjectToDtos(_dbContext.Clubs.Where(club => club.OwnerId == ownerId)).ToListAsync();
    }

    public async Task<PageResult<ClubDto>> GetPageAsync(int page, int pageSize)
    {
        var newestFirst = _dbContext.Clubs.OrderByDescending(club => club.CreatedAt);

        var totalCount = await newestFirst.CountAsync();
        var items = await ProjectToDtos(newestFirst)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<ClubDto>(items, totalCount);
    }

    public Task<ClubMembershipDto?> GetMembershipAsync(Guid clubId, Guid userId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .Select(club => new ClubMembershipDto(
                club.Members.Any(member => member.UserId == userId) ? ClubMembershipStatus.Member
                : club.Status == ClubStatus.Closed ? ClubMembershipStatus.Closed
                : club.JoinRequests.Any(request => request.UserId == userId) ? ClubMembershipStatus.Pending
                : ClubMembershipStatus.None,
                club.Members
                    .Where(member => member.UserId == userId)
                    .Select(member => (ClubRole?)member.Role)
                    .FirstOrDefault()))
            .FirstOrDefaultAsync();
    }

    public Task<List<ClubJoinRequestDto>> GetJoinRequestsAsync(Guid clubId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .SelectMany(club => club.JoinRequests)
            .Select(request => new ClubJoinRequestDto(request.UserId, null, request.SentAt))
            .ToListAsync();
    }

    public Task<List<ClubMemberDto>> GetMembersAsync(Guid clubId)
    {
        return _dbContext.Clubs
            .Where(club => club.Id == clubId)
            .SelectMany(club => club.Members)
            .Select(member => new ClubMemberDto(member.UserId, null, member.Role))
            .ToListAsync();
    }

    private static IQueryable<ClubDto> ProjectToDtos(IQueryable<Club> clubs)
    {
        return clubs
            .Select(club => new ClubDto(club.Id, club.OwnerId, club.Name, club.Description, club.Images));
    }
}
