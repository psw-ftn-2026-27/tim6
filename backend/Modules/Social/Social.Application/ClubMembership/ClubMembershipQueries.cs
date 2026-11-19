using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubMembership;

public sealed class ClubMembershipQueries
{
    private readonly IClubReadRepository _clubReadRepository;

    public ClubMembershipQueries(IClubReadRepository clubReadRepository)
    {
        _clubReadRepository = clubReadRepository;
    }

    public async Task<ClubMembershipDto> GetMembershipAsync(Guid clubId, Guid userId)
    {
        var membership = await _clubReadRepository.GetMembershipAsync(clubId, userId);
        if (membership is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }

        return membership;
    }

    public Task<List<ReceivedClubInvitationDto>> GetMyInvitationsAsync(Guid userId)
    {
        return _clubReadRepository.GetInvitationsForUserAsync(userId);
    }
}
