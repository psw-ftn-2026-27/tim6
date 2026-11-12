using Games.Application.SocialChallengeAuthoring;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<List<SocialChallengeDto>> GetAllAsync();
}
