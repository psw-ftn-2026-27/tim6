using System.Net;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePublishing;

public class SocialChallengePublishingCommandTests : BaseIntegrationTest
{
    public SocialChallengePublishingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_with_a_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.LocatedDraftFortressSunset.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.LocatedDraftFortressSunset.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Draft_without_a_location_cannot_be_published_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.DraftSquareGathering.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.DraftSquareGathering.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Unknown_social_challenge_cannot_be_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var publishedCountBefore = arrangeContext.SocialChallenges.Count(challenge => challenge.Status == ChallengeStatus.Published);

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count(challenge => challenge.Status == ChallengeStatus.Published).Should().Be(publishedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.LocatedDraftFortressSunset.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.LocatedDraftFortressSunset.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedRiversidePicnic.Id);
        stored.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Fact]
    public async Task Draft_cannot_be_retired_and_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.LocatedDraftFortressSunset.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.LocatedDraftFortressSunset.Id);
        stored.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Fact]
    public async Task Unknown_social_challenge_cannot_be_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var retiredCountBefore = arrangeContext.SocialChallenges.Count(challenge => challenge.Status == ChallengeStatus.Retired);

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count(challenge => challenge.Status == ChallengeStatus.Retired).Should().Be(retiredCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredWinterSkating.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.RetiredWinterSkating.Id);
        stored.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Fact]
    public async Task Published_social_challenge_cannot_be_archived_and_stays_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.PublishedRiversidePicnic.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.SocialChallenges.Single(challenge => challenge.Id == SocialChallengeSeed.PublishedRiversidePicnic.Id);
        stored.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public async Task Unknown_social_challenge_cannot_be_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var archivedCountBefore = arrangeContext.SocialChallenges.Count(challenge => challenge.Status == ChallengeStatus.Archived);

        var response = await client.PostAsync($"/api/games/social-challenges/{Guid.NewGuid()}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.SocialChallenges.Count(challenge => challenge.Status == ChallengeStatus.Archived).Should().Be(archivedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_social_challenge()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredWinterSkating.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_social_challenge()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/social-challenges/{SocialChallengeSeed.RetiredWinterSkating.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
