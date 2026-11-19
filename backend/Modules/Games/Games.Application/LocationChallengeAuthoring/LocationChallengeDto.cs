using Games.Application.Challenges;
using Games.Domain.Challenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeDto(
    Guid Id,
    string Name,
    int Xp,
    ChallengeStatus Status,
    List<HiddenPointDto> HiddenPoints,
    LocationDto? Location);
