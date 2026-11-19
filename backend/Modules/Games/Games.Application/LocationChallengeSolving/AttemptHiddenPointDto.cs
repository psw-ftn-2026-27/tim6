using Games.Application.Challenges;

namespace Games.Application.LocationChallengeSolving;

// Location stays null until the hidden point is revealed.
public sealed record AttemptHiddenPointDto(Guid Id, string Image, bool IsRevealed, LocationDto? Location);
