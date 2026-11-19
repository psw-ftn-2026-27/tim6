using Games.Application.ChallengeProgresses;
using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.ChallengeProgresses;
using Games.Domain.Challenges;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IChallengeProgressRepository _progressRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizSolvingService(
        IQuizRepository quizRepository,
        IChallengeProgressRepository progressRepository,
        IQuizAttemptRepository attemptRepository,
        IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _progressRepository = progressRepository;
        _attemptRepository = attemptRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> SubmitAttemptAsync(Guid quizId, Guid touristId, SubmitAttemptDto dto)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        quiz.EnsureCanBeActivatedFrom(new Location(dto.Position.Latitude, dto.Position.Longitude));

        var answers = dto.Answers
            .Select(answer => new SelectedAnswer(answer.QuestionIndex, answer.OptionIndex))
            .ToList();
        var score = quiz.Grade(answers);

        var progress = await _progressRepository.GetAsync(quiz.Id, touristId);
        if (progress is null)
        {
            progress = new ChallengeProgress(quiz.Id, touristId);
            _progressRepository.Add(progress);
        }
        var finishedAt = DateTime.UtcNow;
        var awardedXp = progress.RecordResult(score.Xp, score.AllCorrect, finishedAt);

        var attempt = new QuizAttempt(quiz.Id, touristId, answers, score, awardedXp, finishedAt);
        _attemptRepository.Add(attempt);
        await _unitOfWork.SaveChangesAsync();

        return attempt.Id;
    }
}
