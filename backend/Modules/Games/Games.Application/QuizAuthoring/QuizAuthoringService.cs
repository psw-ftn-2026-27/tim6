using Games.Application.Quizzes;
using Games.Domain.Challenges;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizAuthoringService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(CreateQuizDto dto)
    {
        var quiz = new Quiz(dto.Name, dto.Instructions, dto.Xp, ToQuestions(dto.Questions));
        if (dto.Location is not null)
        {
            quiz.PlaceAt(new Location(dto.Location.Latitude, dto.Location.Longitude));
        }

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return quiz.Id;
    }

    public async Task EditAsync(Guid quizId, UpdateQuizDto dto)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.Edit(dto.Name, dto.Instructions, dto.Xp, ToQuestions(dto.Questions));
        if (dto.Location is null)
        {
            quiz.RemoveLocation();
        }
        else
        {
            quiz.PlaceAt(new Location(dto.Location.Latitude, dto.Location.Longitude));
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid quizId)
    {
        var quiz = await GetQuizAsync(quizId);

        quiz.Delete();
        _quizRepository.Remove(quiz);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }

    private static List<Question> ToQuestions(List<QuestionDto> questions)
    {
        return questions
            .Select(question => new Question(
                question.Text,
                question.Options.Select(option => new AnswerOption(option.Text, option.IsCorrect)).ToList()))
            .ToList();
    }
}
