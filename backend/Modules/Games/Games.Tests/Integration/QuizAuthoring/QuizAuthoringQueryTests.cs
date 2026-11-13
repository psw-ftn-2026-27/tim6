using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringQueryTests : BaseIntegrationTest
{
    public QuizAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_every_quiz_with_its_questions_and_answer_options_newest_first()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var quizzes = await client.GetFromJsonAsync<List<QuizDto>>("/api/games/quizzes", JsonOptions);

        var expected = QuizSeed.All
            .OrderByDescending(quiz => quiz.CreatedAt.Ticks / TimeSpan.TicksPerMicrosecond)
            .ThenBy(quiz => quiz.Id)
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Xp,
                quiz.Status,
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Text,
                        question.Options.Select(option => new AnswerOptionDto(option.Text, option.IsCorrect)).ToList()))
                    .ToList()));
        quizzes.Should().BeEquivalentTo(expected, options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }
}
