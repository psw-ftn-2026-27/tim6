using Games.Application.Challenges;

namespace Games.Application.ChallengeMap;

public sealed record QuizOnMapDto(Guid Id, string Name, int Xp, LocationDto Location, bool IsSolved);
