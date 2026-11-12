using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengeAuthoring;

public sealed record SocialChallengeDto(
    Guid Id,
    string Name,
    ChallengeStatus Status,
    DailyTimeWindowDto TimeWindow,
    int RequiredPeople,
    int RangeInMeters,
    int Xp,
    string ExitMessage,
    LocationDto? Location);
