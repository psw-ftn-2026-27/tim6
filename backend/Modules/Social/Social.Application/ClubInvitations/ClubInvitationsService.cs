using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;
using Social.Domain.Clubs;

namespace Social.Application.ClubInvitations;

public sealed class ClubInvitationsService
{
    private readonly IClubRepository _clubRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityApi _identityApi;

    public ClubInvitationsService(IClubRepository clubRepository, IUnitOfWork unitOfWork, IIdentityApi identityApi)
    {
        _clubRepository = clubRepository;
        _unitOfWork = unitOfWork;
        _identityApi = identityApi;
    }

    public async Task InviteAsync(Guid clubId, Guid userId, InviteDto dto)
    {
        var club = await GetClubAsync(clubId);
        var invitee = await _identityApi.GetUserByEmailAsync(dto.Email);
        if (invitee is null)
        {
            throw new NotFoundException($"User with email {dto.Email} does not exist.");
        }

        club.Invite(userId, invitee.UserId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawInvitationAsync(Guid clubId, Guid userId, Guid inviteeId)
    {
        var club = await GetClubAsync(clubId);

        club.WithdrawInvitation(userId, inviteeId);
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
