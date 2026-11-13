namespace Games.Application.SocialChallengeAuthoring;

public sealed record UpdateSocialChallengeDto(string Name, DailyTimeWindowDto TimeWindow, int RequiredPeople,
    int RangeInMeters, int Xp, string ExitMessage, LocationDto? Location);
