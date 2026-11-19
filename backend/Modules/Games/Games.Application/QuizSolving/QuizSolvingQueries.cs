using Games.Application.ChallengeProgresses;
using Games.Application.Challenges;
using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.Challenges;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingQueries
{
    private readonly IQuizRepository _quizRepository;
    private readonly IChallengeProgressRepository _progressRepository;
    private readonly IQuizAttemptReadRepository _attemptReadRepository;

    public QuizSolvingQueries(
        IQuizRepository quizRepository,
        IChallengeProgressRepository progressRepository,
        IQuizAttemptReadRepository attemptReadRepository)
    {
        _quizRepository = quizRepository;
        _progressRepository = progressRepository;
        _attemptReadRepository = attemptReadRepository;
    }

    public async Task<QuizForSolvingDto> GetQuizForSolvingAsync(Guid quizId, Guid touristId, LocationDto position)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        quiz.EnsureCanBeActivatedFrom(new Location(position.Latitude, position.Longitude));
        var progress = await _progressRepository.GetAsync(quiz.Id, touristId);
        if (progress is not null && progress.IsSolved())
        {
            throw new DomainException("A solved quiz cannot be activated again.");
        }

        return new QuizForSolvingDto(
            quiz.Id,
            quiz.Name,
            quiz.Instructions,
            quiz.Questions
                .Select(question => new QuestionForSolvingDto(
                    question.Text,
                    question.Options.Select(option => option.Text).ToList()))
                .ToList());
    }

    public async Task<QuizAttemptResultDto> GetAttemptResultAsync(Guid attemptId, Guid touristId)
    {
        var result = await _attemptReadRepository.GetResultAsync(attemptId, touristId);
        if (result is null)
        {
            throw new NotFoundException($"Quiz attempt {attemptId} does not exist.");
        }
        return result;
    }
}
