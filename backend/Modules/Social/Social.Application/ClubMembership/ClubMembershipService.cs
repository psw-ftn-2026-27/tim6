using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubMembershipService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RequestToJoinAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.RequestToJoin(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawJoinRequestAsync(Guid clubId, Guid userId)
    {
        var club = await GetClubAsync(clubId);

        club.WithdrawJoinRequest(userId);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Club> GetClubAsync(Guid clubId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        return club;
    }
}
