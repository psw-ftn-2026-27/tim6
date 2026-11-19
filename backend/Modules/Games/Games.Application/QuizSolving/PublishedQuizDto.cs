namespace Games.Application.QuizSolving;

public sealed record PublishedQuizDto(Guid Id, string Name, int Xp, double DistanceInMeters, bool IsSolved);
