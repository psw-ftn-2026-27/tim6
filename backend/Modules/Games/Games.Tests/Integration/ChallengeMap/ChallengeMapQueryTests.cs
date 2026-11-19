using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.ChallengeMap;
using Games.Domain.Challenges;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.ChallengeMap;

public class ChallengeMapQueryTests : BaseIntegrationTest
{
    public ChallengeMapQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Map_holds_only_published_quizzes_and_social_challenges()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var challenges = await client.GetFromJsonAsync<List<ChallengeOnMapDto>>("/api/games/challenge-map", JsonOptions);

        var expectedIds = QuizSeed.All
            .Where(quiz => quiz.Status == ChallengeStatus.Published)
            .Select(quiz => quiz.Id)
            .Concat(SocialChallengeSeed.All
                .Where(challenge => challenge.Status == ChallengeStatus.Published)
                .Select(challenge => challenge.Id));
        challenges!.Select(challenge => challenge.Id).Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task Challenge_the_explorer_solved_is_marked_solved_and_cannot_be_activated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedMuseumStreet;

        var challenges = await client.GetFromJsonAsync<List<ChallengeOnMapDto>>(FormattableString.Invariant(
            $"/api/games/challenge-map?latitude={quiz.Location!.Latitude}&longitude={quiz.Location.Longitude}"), JsonOptions);

        var solved = challenges!.Single(challenge => challenge.Id == quiz.Id);
        solved.IsSolved.Should().BeTrue();
        solved.CanActivate.Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_near_an_unsolved_quiz_can_activate_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedFortressGates;

        var challenges = await client.GetFromJsonAsync<List<ChallengeOnMapDto>>(FormattableString.Invariant(
            $"/api/games/challenge-map?latitude={quiz.Location!.Latitude}&longitude={quiz.Location.Longitude}"), JsonOptions);

        var nearby = challenges!.Single(challenge => challenge.Id == quiz.Id);
        nearby.Kind.Should().Be(ChallengeKind.Quiz);
        nearby.IsSolved.Should().BeFalse();
        nearby.DistanceInMeters.Should().BeApproximately(0, 1);
        nearby.CanActivate.Should().BeTrue();
    }

    [Fact]
    public async Task Quiz_farther_than_fifty_meters_cannot_be_activated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var quiz = QuizSeed.PublishedFortressGates;
        var explorerLocation = new Location(quiz.Location!.Latitude + 0.0006, quiz.Location.Longitude);

        var challenges = await client.GetFromJsonAsync<List<ChallengeOnMapDto>>(FormattableString.Invariant(
            $"/api/games/challenge-map?latitude={explorerLocation.Latitude}&longitude={explorerLocation.Longitude}"), JsonOptions);

        var distant = challenges!.Single(challenge => challenge.Id == quiz.Id);
        distant.DistanceInMeters.Should().BeGreaterThan(50);
        distant.CanActivate.Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_without_a_position_sees_challenges_without_distance_and_can_activate_none()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var challenges = await client.GetFromJsonAsync<List<ChallengeOnMapDto>>("/api/games/challenge-map", JsonOptions);

        challenges.Should().NotBeEmpty();
        challenges.Should().OnlyContain(challenge => challenge.DistanceInMeters == null && !challenge.CanActivate);
    }

    [Fact]
    public async Task Anonymous_user_cannot_open_the_challenge_map()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/games/challenge-map");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_open_the_challenge_map()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/games/challenge-map");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
