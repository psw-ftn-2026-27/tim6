using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubModeration;

public sealed class ClubModerationService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ClubModerationService(IClubRepository clubRepository, IUnitOfWork unitOfWork)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task AcceptJoinRequestAsync(Guid clubId, Guid userId, Guid applicantId)
    {
        var club = await GetClubAsync(clubId);

        club.AcceptJoinRequest(userId, applicantId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RejectJoinRequestAsync(Guid clubId, Guid userId, Guid applicantId)
    {
        var club = await GetClubAsync(clubId);

        club.RejectJoinRequest(userId, applicantId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PromoteToModeratorAsync(Guid clubId, Guid userId, Guid memberId)
    {
        var club = await GetClubAsync(clubId);

        club.PromoteToModerator(userId, memberId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid clubId, Guid userId, Guid memberId)
    {
        var club = await GetClubAsync(clubId);

        club.RemoveMember(userId, memberId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task TransferOwnershipAsync(Guid clubId, Guid userId, Guid newOwnerId)
    {
        var club = await GetClubAsync(clubId);

        club.TransferOwnership(userId, newOwnerId);
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
