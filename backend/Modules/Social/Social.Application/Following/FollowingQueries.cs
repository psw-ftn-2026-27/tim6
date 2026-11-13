namespace Social.Application.Following;

public sealed class FollowingQueries
{
    private readonly IFollowReadRepository _followReadRepository;

    public FollowingQueries(IFollowReadRepository followReadRepository)
    {
        _followReadRepository = followReadRepository;
    }

    public async Task<FollowStatusDto> GetFollowStatusAsync(Guid userId, Guid otherUserId)
    {
        return new FollowStatusDto(await _followReadRepository.GetFollowStatusAsync(userId, otherUserId));
    }

    public Task<List<SentFollowRequestDto>> GetSentRequestsAsync(Guid userId)
    {
        return _followReadRepository.GetSentRequestsAsync(userId);
    }

    public Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid userId)
    {
        return _followReadRepository.GetFollowedUsersAsync(userId);
    }

    public Task<List<ReceivedFollowRequestDto>> GetReceivedRequestsAsync(Guid userId)
    {
        return _followReadRepository.GetReceivedRequestsAsync(userId);
    }

    public Task<List<FollowerDto>> GetFollowersAsync(Guid userId)
    {
        return _followReadRepository.GetFollowersAsync(userId);
    }
}
