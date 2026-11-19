using Games.Application.Challenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record CreateLocationChallengeDto(string Name, int Xp, List<HiddenPointDto> HiddenPoints,
    LocationDto? Location);
