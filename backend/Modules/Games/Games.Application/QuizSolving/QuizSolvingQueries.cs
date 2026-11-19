using Games.Application.ChallengeProgresses;
using Games.Application.Challenges;
using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.Challenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IChallengeProgressRepository _progressRepository;
    private readonly IQuizAttemptReadRepository _attemptReadRepository;

    public QuizSolvingQueries(
        IQuizReadRepository quizReadRepository,
        IQuizRepository quizRepository,
        IChallengeProgressRepository progressRepository,
        IQuizAttemptReadRepository attemptReadRepository)
    {
        _quizReadRepository = quizReadRepository;
        _quizRepository = quizRepository;
        _progressRepository = progressRepository;
        _attemptReadRepository = attemptReadRepository;
    }

    public async Task<List<PublishedQuizDto>> GetPublishedQuizzesAsync(Guid touristId, LocationDto position)
    {
        var touristLocation = new Location(position.Latitude, position.Longitude);
        var quizzes = await _quizReadRepository.GetPublishedAsync(touristId);

        return quizzes
            .Select(quiz => new PublishedQuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Xp,
                new Location(quiz.Location.Latitude, quiz.Location.Longitude).DistanceTo(touristLocation),
                Quiz.ActivationRadius,
                quiz.IsSolved))
            .OrderBy(quiz => quiz.DistanceInMeters)
            .ToList();
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
