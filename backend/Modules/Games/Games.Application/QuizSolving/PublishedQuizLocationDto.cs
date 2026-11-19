using Games.Application.Challenges;

namespace Games.Application.QuizSolving;

public sealed record PublishedQuizLocationDto(Guid Id, string Name, int Xp, LocationDto Location, bool IsSolved);
