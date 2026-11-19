using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeSolving;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeSolving;

public class SocialChallengeSolvingQueryTests : BaseIntegrationTest
{
    public SocialChallengeSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_who_solved_a_social_challenge_sees_the_earned_xp_and_the_exit_message()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var challenge = SocialChallengeSeed.PublishedRiversidePicnic;

        var state = await client.GetFromJsonAsync<SocialChallengeRunStateDto>(
            $"/api/games/social-challenges/{challenge.Id}/run-state", JsonOptions);

        state!.Id.Should().Be(challenge.Id);
        state.Status.Should().Be(SolvingStatus.Solved);
        state.EarnedXp.Should().Be(ChallengeProgressSeed.ExplorerSolvedRiversidePicnic.EarnedXp);
        state.ExitMessage.Should().Be(challenge.ExitMessage);
    }

    [Fact]
    public async Task Anonymous_user_cannot_see_the_run_state_of_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/run-state");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_see_the_run_state_of_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync(
            $"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/run-state");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
