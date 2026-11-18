using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipCommandTests : BaseIntegrationTest
{
    public ClubMembershipCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Join_request_of_a_non_member_to_an_active_club_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}/join-request", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikingClub.Id);
        stored.JoinRequests.Should().ContainSingle(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task Second_join_request_of_the_same_user_to_the_same_club_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Clubs
            .Single(club => club.Id == ClubSeed.ClubWithSecondExplorersJoinRequest.Id)
            .JoinRequests.Count;

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.ClubWithSecondExplorersJoinRequest.Id}/join-request", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.ClubWithSecondExplorersJoinRequest.Id);
        stored.JoinRequests.Should().HaveCount(requestCountBefore);
    }

    [Fact]
    public async Task Join_request_to_a_club_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var unknownClubId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/social/clubs/{unknownClubId}/join-request", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == unknownClubId).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_request_to_join_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}/join-request", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Withdrawn_join_request_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.ClubWithSecondExplorersJoinRequest.Id}/join-request");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.ClubWithSecondExplorersJoinRequest.Id);
        stored.JoinRequests.Should().NotContain(request => request.UserId == WellKnownUsers.SecondExplorer);
    }

    [Fact]
    public async Task User_without_a_pending_request_has_nothing_to_withdraw()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var requestCountBefore = arrangeContext.Clubs
            .Single(club => club.Id == ClubSeed.ClubWithSecondExplorersJoinRequest.Id)
            .JoinRequests.Count;

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.ClubWithSecondExplorersJoinRequest.Id}/join-request");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.ClubWithSecondExplorersJoinRequest.Id);
        stored.JoinRequests.Should().HaveCount(requestCountBefore);
    }

    [Fact]
    public async Task Join_request_to_a_club_that_does_not_exist_cannot_be_withdrawn()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var unknownClubId = Guid.NewGuid();

        var response = await client.DeleteAsync($"/api/social/clubs/{unknownClubId}/join-request");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == unknownClubId).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_a_join_request()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.ClubWithSecondExplorersJoinRequest.Id}/join-request");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
