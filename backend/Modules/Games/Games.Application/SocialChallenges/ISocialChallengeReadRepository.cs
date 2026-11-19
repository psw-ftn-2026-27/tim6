using Games.Application.ChallengeMap;
using Games.Application.SocialChallengeAuthoring;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<List<SocialChallengeDto>> GetAllAsync();
    Task<List<SocialChallengeOnMapDto>> GetPublishedForMapAsync(Guid touristId);
}
