using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.FollowRelationships;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

public class FollowingCommandTests : BaseIntegrationTest
{
    public FollowingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_sends_a_follow_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync(
            $"/api/social/users/{WellKnownUsers.Explorer}/follow-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var requestId = await response.Content.ReadFromJsonAsync<Guid>();
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships
            .Single(relationship => relationship.Requests.Any(request => request.Id == requestId));
        stored.Requests.Should().Contain(request =>
            request.Id == requestId
            && request.SenderId == WellKnownUsers.Administrator
            && request.RecipientId == WellKnownUsers.Explorer
            && request.Status == FollowRequestStatus.Pending);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_send_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync(
            $"/api/social/users/{WellKnownUsers.Explorer}/follow-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Crossed_requests_sent_through_HTTP_stay_pending()
    {
        var relationship = FollowRelationshipSeed.SecondExplorerRequestedExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/users/{WellKnownUsers.SecondExplorer}/follow-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.Requests.Should().HaveCount(relationship.Requests.Count + 1);
        stored.Requests.Should().OnlyContain(request => request.Status == FollowRequestStatus.Pending);
        stored.GetFollowStatusFor(WellKnownUsers.Explorer).Should().Be(FollowStatus.Pending);
        stored.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Pending);
    }

    [Fact]
    public async Task Sending_a_second_request_while_one_is_pending_is_rejected_and_nothing_changes()
    {
        var relationship = FollowRelationshipSeed.SecondExplorerRequestedExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/users/{WellKnownUsers.Explorer}/follow-requests", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.Requests.Should().HaveCount(relationship.Requests.Count);
    }

    [Fact]
    public async Task Sender_cancels_a_pending_request()
    {
        var relationship = FollowRelationshipSeed.SecondExplorerRequestedExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync(
            $"/api/social/follow-requests/{relationship.Requests.Single().Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.Requests.Should().HaveCount(relationship.Requests.Count - 1);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_cancel_a_follow_request()
    {
        var requestId = FollowRelationshipSeed.SecondExplorerRequestedExplorer.Requests.Single().Id;
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task User_stops_following_another_user()
    {
        var relationship = FollowRelationshipSeed.AdministratorAndSecondExplorerFollowEachOther;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.GetFollowStatusFor(WellKnownUsers.Administrator).Should().Be(FollowStatus.Follow);
        stored.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Following);
        stored.Requests.Should().HaveCount(relationship.Requests.Count);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_stop_following_a_user()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/following/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Recipient_accepts_a_pending_request()
    {
        var relationship = FollowRelationshipSeed.SecondExplorerRequestedExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{relationship.Requests.Single().Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Following);
    }

    [Fact]
    public async Task A_request_that_is_not_pending_cannot_be_accepted_through_HTTP()
    {
        var relationship = FollowRelationshipSeed.AdministratorAndSecondExplorerFollowEachOther;
        var acceptedRequest = relationship.Requests.Single(request => request.SenderId == WellKnownUsers.Administrator);
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/social/follow-requests/{acceptedRequest.Id}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_accept_a_follow_request()
    {
        var requestId = FollowRelationshipSeed.SecondExplorerRequestedExplorer.Requests.Single().Id;
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Recipient_rejects_a_pending_request()
    {
        var relationship = FollowRelationshipSeed.SecondExplorerRequestedExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{relationship.Requests.Single().Id}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.Requests.Single().Status.Should().Be(FollowRequestStatus.Rejected);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_reject_a_follow_request()
    {
        var requestId = FollowRelationshipSeed.SecondExplorerRequestedExplorer.Requests.Single().Id;
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Accept_and_follow_back_establishes_mutual_follow()
    {
        var relationship = FollowRelationshipSeed.SecondExplorerRequestedExplorer;
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync(
            $"/api/social/follow-requests/{relationship.Requests.Single().Id}/accept-and-follow-back", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.GetFollowStatusFor(WellKnownUsers.Explorer).Should().Be(FollowStatus.Following);
        stored.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Following);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_accept_and_follow_back()
    {
        var requestId = FollowRelationshipSeed.SecondExplorerRequestedExplorer.Requests.Single().Id;
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/follow-requests/{requestId}/accept-and-follow-back", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task User_removes_a_follower()
    {
        var relationship = FollowRelationshipSeed.AdministratorAndSecondExplorerFollowEachOther;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.FollowRelationships.Single(candidate => candidate.Id == relationship.Id);
        stored.GetFollowStatusFor(WellKnownUsers.SecondExplorer).Should().Be(FollowStatus.Follow);
        stored.GetFollowStatusFor(WellKnownUsers.Administrator).Should().Be(FollowStatus.Following);
    }

    [Fact]
    public async Task Unauthenticated_user_cannot_remove_a_follower()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/followers/{WellKnownUsers.SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
