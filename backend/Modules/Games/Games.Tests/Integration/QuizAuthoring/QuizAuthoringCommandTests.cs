using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Domain.Challenges;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringCommandTests : BaseIntegrationTest
{
    public QuizAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    private static List<QuestionDto> CreateQuestions() =>
    [
        new QuestionDto("Koja reka protiče kroz grad?", [new AnswerOptionDto("Dunav", true), new AnswerOptionDto("Sava", false)]),
        new QuestionDto("Koliko mostova ima grad?", [new AnswerOptionDto("Dva", false), new AnswerOptionDto("Tri", true), new AnswerOptionDto("Četiri", false)])
    ];

    private static List<Question> ToQuestions(List<QuestionDto> questions) =>
        questions
            .Select(question => new Question(
                question.Text,
                question.Options.Select(option => new AnswerOption(option.Text, option.IsCorrect)).ToList()))
            .ToList();

    [Fact]
    public async Task Administrator_creates_a_draft_quiz_stored_with_its_questions_and_answer_options()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore + 1);
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == createdId);
        stored.Name.Should().Be("Obala Dunava");
        stored.Instructions.Should().Be("Prošetajte kejom i odgovorite na pitanja.");
        stored.Xp.Should().Be(120);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Questions.Should().BeEquivalentTo(ToQuestions(request.Questions),
            options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Quiz_without_questions_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, []);
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_quiz()
    {
        var client = Factory.CreateClient();
        var request = new CreateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());

        var response = await client.PostAsJsonAsync("/api/games/quizzes", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Edited_draft_is_stored_with_new_data_and_new_questions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == QuizSeed.DraftFortressHistory.Id);
        stored.Name.Should().Be("Obala Dunava");
        stored.Instructions.Should().Be("Prošetajte kejom i odgovorite na pitanja.");
        stored.Xp.Should().Be(120);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Questions.Should().BeEquivalentTo(ToQuestions(request.Questions),
            options => options.ComparingRecordsByMembers().WithStrictOrdering());
    }

    [Fact]
    public async Task Unknown_quiz_cannot_be_edited()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Any(quiz => quiz.Name == "Obala Dunava").Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_edit_a_quiz()
    {
        var client = Factory.CreateClient();
        var request = new UpdateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_edit_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateQuizDto("Obala Dunava", "Prošetajte kejom i odgovorite na pitanja.", 120, CreateQuestions());

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deleted_draft_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore - 1);
        assertContext.Quizzes.Any(quiz => quiz.Id == QuizSeed.DraftFortressHistory.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_quiz_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressHistory.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
