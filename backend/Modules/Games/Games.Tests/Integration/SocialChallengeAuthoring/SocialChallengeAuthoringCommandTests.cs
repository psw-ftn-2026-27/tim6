using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengeAuthoring;

public class SocialChallengeAuthoringCommandTests : BaseIntegrationTest
{
    public SocialChallengeAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_draft_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Jutarnje trčanje", new DailyTimeWindowDto(new TimeOnly(6, 0), new TimeOnly(8, 0)), 4, 100, 150, "Zajedno ste istrčali jutro!");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<SocialChallengeDto>(JsonOptions);
        created!.Status.Should().Be(ChallengeStatus.Draft);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore + 1);
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == created.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
        stored.Name.Should().Be("Jutarnje trčanje");
        stored.TimeWindow.Should().Be(new DailyTimeWindow(new TimeOnly(6, 0), new TimeOnly(8, 0)));
        stored.RequiredPeople.Should().Be(4);
        stored.RangeInMeters.Should().Be(100);
        stored.Xp.Should().Be(150);
        stored.ExitMessage.Should().Be("Zajedno ste istrčali jutro!");
    }

    [Fact]
    public async Task Social_challenge_for_a_single_person_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateSocialChallengeDto("Jutarnje trčanje", new DailyTimeWindowDto(new TimeOnly(6, 0), new TimeOnly(8, 0)), 1, 100, 150, "Zajedno ste istrčali jutro!");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_social_challenge()
    {
        var client = Factory.CreateClient();
        var request = new CreateSocialChallengeDto("Jutarnje trčanje", new DailyTimeWindowDto(new TimeOnly(6, 0), new TimeOnly(8, 0)), 4, 100, 150, "Zajedno ste istrčali jutro!");

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateSocialChallengeDto("Jutarnje trčanje", new DailyTimeWindowDto(new TimeOnly(6, 0), new TimeOnly(8, 0)), 4, 100, 150, "Zajedno ste istrčali jutro!");

        var response = await client.PostAsJsonAsync("/api/games/social-challenges", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Edited_draft_is_stored_with_new_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Večernje okupljanje", new DailyTimeWindowDto(new TimeOnly(23, 0), new TimeOnly(1, 0)), 8, 120, 300, "Okupili ste se uveče!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSquareGathering.Id);
        stored.Name.Should().Be("Večernje okupljanje");
        stored.TimeWindow.Should().Be(new DailyTimeWindow(new TimeOnly(23, 0), new TimeOnly(1, 0)));
        stored.RequiredPeople.Should().Be(8);
        stored.RangeInMeters.Should().Be(120);
        stored.Xp.Should().Be(300);
        stored.ExitMessage.Should().Be("Okupili ste se uveče!");
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Unknown_social_challenge_cannot_be_edited()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateSocialChallengeDto("Večernje okupljanje", new DailyTimeWindowDto(new TimeOnly(23, 0), new TimeOnly(1, 0)), 8, 120, 300, "Okupili ste se uveče!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Any(challenge => challenge.Name == "Večernje okupljanje").Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_edit_a_social_challenge()
    {
        var client = Factory.CreateClient();
        var request = new UpdateSocialChallengeDto("Večernje okupljanje", new DailyTimeWindowDto(new TimeOnly(23, 0), new TimeOnly(1, 0)), 8, 120, 300, "Okupili ste se uveče!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_edit_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateSocialChallengeDto("Večernje okupljanje", new DailyTimeWindowDto(new TimeOnly(23, 0), new TimeOnly(1, 0)), 8, 120, 300, "Okupili ste se uveče!");

        var response = await client.PutAsJsonAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Deleted_draft_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore - 1);
        assertContext.SocialChallenges.Any(challenge => challenge.Id == SocialChallengeSeed.DraftSquareGathering.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_social_challenge_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var challengeCountBefore = arrangeContext.SocialChallenges.Count();

        var response = await client.DeleteAsync($"/api/games/social-challenges/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count().Should().Be(challengeCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
