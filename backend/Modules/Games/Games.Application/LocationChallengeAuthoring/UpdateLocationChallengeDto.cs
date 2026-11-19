using Games.Application.Challenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record UpdateLocationChallengeDto(string Name, int Xp, List<HiddenPointDto> HiddenPoints,
    LocationDto? Location);
