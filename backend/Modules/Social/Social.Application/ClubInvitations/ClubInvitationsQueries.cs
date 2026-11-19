using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubInvitations;

public sealed class ClubInvitationsQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IIdentityApi _identityApi;

    public ClubInvitationsQueries(
        IClubReadRepository clubReadRepository,
        IClubRepository clubRepository,
        IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _clubRepository = clubRepository;
        _identityApi = identityApi;
    }

    public async Task<List<ClubInvitationDto>> GetInvitationsAsync(Guid clubId, Guid userId)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        if (club.OwnerId != userId)
        {
            throw new DomainException("Only the owner of the club can see its invitations.");
        }

        var invitations = await _clubReadRepository.GetInvitationsAsync(clubId);
        var users = await _identityApi.GetUsersByIdsAsync(invitations.Select(invitation => invitation.UserId));

        return invitations
            .Select(invitation => invitation with
            {
                UserEmail = users.TryGetValue(invitation.UserId, out var user)
                    ? user.Email
                    : null
            })
            .ToList();
    }
}
