using Games.Application.Challenges;

namespace Games.Application.SocialChallengeAuthoring;

public sealed record CreateSocialChallengeDto(string Name, DailyTimeWindowDto TimeWindow, int RequiredPeople,
    int RangeInMeters, int Xp, string ExitMessage, LocationDto? Location);
