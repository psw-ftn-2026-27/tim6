using Identity.Contracts;
using Shared.Domain.Exceptions;
using Social.Application.Clubs;

namespace Social.Application.ClubModeration;

public sealed class ClubModerationQueries
{
    private readonly IClubReadRepository _clubReadRepository;
    private readonly IClubRepository _clubRepository;
    private readonly IIdentityApi _identityApi;

    public ClubModerationQueries(
        IClubReadRepository clubReadRepository,
        IClubRepository clubRepository,
        IIdentityApi identityApi)
    {
        _clubReadRepository = clubReadRepository;
        _clubRepository = clubRepository;
        _identityApi = identityApi;
    }

    public async Task<List<ClubJoinRequestDto>> GetJoinRequestsAsync(Guid clubId, Guid userId)
    {
        await EnsureOwnerOrModeratorAsync(clubId, userId,
            "Only the owner or a moderator of the club can see its join requests.");

        var requests = await _clubReadRepository.GetJoinRequestsAsync(clubId);
        var users = await _identityApi.GetUsersByIdsAsync(requests.Select(request => request.UserId));

        return requests
            .Select(request => request with
            {
                UserEmail = users.TryGetValue(request.UserId, out var user)
                    ? user.Email
                    : null
            })
            .ToList();
    }

    public async Task<List<ClubMemberDto>> GetMembersAsync(Guid clubId, Guid userId)
    {
        await EnsureOwnerOrModeratorAsync(clubId, userId,
            "Only the owner or a moderator of the club can see its members.");

        var members = await _clubReadRepository.GetMembersAsync(clubId);
        var users = await _identityApi.GetUsersByIdsAsync(members.Select(member => member.UserId));

        return members
            .Select(member => member with
            {
                UserEmail = users.TryGetValue(member.UserId, out var user)
                    ? user.Email
                    : null
            })
            .ToList();
    }

    private async Task EnsureOwnerOrModeratorAsync(Guid clubId, Guid userId, string deniedMessage)
    {
        var club = await _clubRepository.GetByIdAsync(clubId);
        if (club is null)
        {
            throw new NotFoundException($"Club {clubId} does not exist.");
        }
        if (!club.CanResolveJoinRequests(userId))
        {
            throw new DomainException(deniedMessage);
        }
    }
}
