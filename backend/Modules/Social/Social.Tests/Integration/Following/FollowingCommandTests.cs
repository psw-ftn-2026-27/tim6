using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Following;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

public class FollowingCommandTests : BaseIntegrationTest
{
    private static readonly Guid Administrator = WellKnownUsers.Administrator;
    private static readonly Guid Explorer = WellKnownUsers.Explorer;
    private static readonly Guid SecondExplorer = WellKnownUsers.SecondExplorer;
    private static readonly Guid ThirdExplorer = FollowSeed.ThirdExplorer;

    public FollowingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_unauthenticated_user_cannot_send_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await SendRequestAsync(client, Explorer);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unauthenticated_user_cannot_cancel_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await DeleteAsync(
            client, $"/api/social/follow-requests/{FollowSeed.SecondExplorerPendingRequestToExplorer.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unauthenticated_user_cannot_unfollow_a_user()
    {
        var client = Factory.CreateClient();

        var response = await DeleteAsync(client, $"/api/social/following/{SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unauthenticated_user_cannot_accept_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await PostAsync(
            client, $"/api/social/follow-requests/{FollowSeed.SecondExplorerPendingRequestToExplorer.Id}/accept");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unauthenticated_user_cannot_reject_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await PostAsync(
            client, $"/api/social/follow-requests/{FollowSeed.SecondExplorerPendingRequestToExplorer.Id}/reject");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unauthenticated_user_cannot_accept_and_follow_back_a_follow_request()
    {
        var client = Factory.CreateClient();

        var response = await PostAsync(
            client,
            $"/api/social/follow-requests/{FollowSeed.SecondExplorerPendingRequestToExplorer.Id}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task An_unauthenticated_user_cannot_remove_a_follower()
    {
        var client = Factory.CreateClient();

        var response = await DeleteAsync(client, $"/api/social/followers/{Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ---- Sending requests ----

    [Fact]
    public async Task A_user_can_send_a_follow_request()
    {
        var response = await SendRequestAsync(ClientFor(Administrator), Explorer);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var request = StoredRequest(await response.Content.ReadFromJsonAsync<Guid>());
        request.SenderId.Should().Be(Administrator);
        request.RecipientId.Should().Be(Explorer);
        request.Status.Should().Be(FollowRequestStatus.Pending);
        IsFollowing(Administrator, Explorer).Should().BeFalse();
    }

    [Fact]
    public async Task A_request_is_accepted_immediately_if_the_recipient_already_follows_the_sender()
    {
        var response = await SendRequestAsync(ClientFor(ThirdExplorer), Administrator);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        StoredRequest(await response.Content.ReadFromJsonAsync<Guid>()).Status.Should().Be(FollowRequestStatus.Accepted);
        IsFollowing(ThirdExplorer, Administrator).Should().BeTrue();
    }

    [Fact]
    public async Task Crossed_requests_stay_pending_instead_of_being_accepted_automatically()
    {
        var response = await SendRequestAsync(ClientFor(Explorer), SecondExplorer);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        StoredRequest(await response.Content.ReadFromJsonAsync<Guid>()).Status.Should().Be(FollowRequestStatus.Pending);
        StoredRequest(FollowSeed.SecondExplorerPendingRequestToExplorer.Id).Status.Should().Be(FollowRequestStatus.Pending);
        IsFollowing(Explorer, SecondExplorer).Should().BeFalse();
        IsFollowing(SecondExplorer, Explorer).Should().BeFalse();
    }

    [Fact]
    public async Task A_second_request_is_rejected_while_one_is_still_pending()
    {
        var before = GetStoredState();

        var response = await SendRequestAsync(ClientFor(SecondExplorer), Explorer);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_user_cannot_send_a_request_to_a_user_they_already_follow()
    {
        var before = GetStoredState();

        var response = await SendRequestAsync(ClientFor(Administrator), ThirdExplorer);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_user_cannot_send_a_request_to_themselves()
    {
        var before = GetStoredState();

        var response = await SendRequestAsync(ClientFor(Administrator), Administrator);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_new_request_can_be_sent_after_the_previous_one_was_rejected()
    {
        var rejectedRequestId = FollowSeed.ExplorerRejectedRequestToSecondExplorer.Id;

        var response = await SendRequestAsync(ClientFor(Explorer), SecondExplorer);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        StoredRequest(rejectedRequestId).Status.Should().Be(FollowRequestStatus.Rejected);
        StoredRequest(await response.Content.ReadFromJsonAsync<Guid>()).Status.Should().Be(FollowRequestStatus.Pending);
    }

    // ---- Cancelling requests ----

    [Fact]
    public async Task The_sender_can_cancel_a_pending_request()
    {
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await DeleteAsync(ClientFor(SecondExplorer), $"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        GetStoredState().Requests.Should().NotContain(request => request.Id == requestId);
    }

    [Fact]
    public async Task Only_the_sender_can_cancel_a_request()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await DeleteAsync(ClientFor(Explorer), $"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task An_answered_request_cannot_be_cancelled()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.AdministratorAcceptedRequestToSecondExplorer.Id;

        var response = await DeleteAsync(ClientFor(Administrator), $"/api/social/follow-requests/{requestId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_request_that_does_not_exist_cannot_be_cancelled()
    {
        var before = GetStoredState();

        var response = await DeleteAsync(ClientFor(Administrator), $"/api/social/follow-requests/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    // ---- Unfollowing ----

    [Fact]
    public async Task A_user_can_unfollow_a_followed_user()
    {
        var requestsBefore = GetStoredState().Requests;

        var response = await DeleteAsync(ClientFor(Administrator), $"/api/social/following/{SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        IsFollowing(Administrator, SecondExplorer).Should().BeFalse();
        IsFollowing(SecondExplorer, Administrator).Should().BeTrue();
        GetStoredState().Requests.Should().BeEquivalentTo(requestsBefore);
    }

    [Fact]
    public async Task A_user_cannot_unfollow_a_user_they_have_no_relationship_with()
    {
        var before = GetStoredState();

        var response = await DeleteAsync(ClientFor(Administrator), $"/api/social/following/{Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_user_cannot_unfollow_a_user_who_only_has_a_pending_request()
    {
        var before = GetStoredState();

        var response = await DeleteAsync(ClientFor(SecondExplorer), $"/api/social/following/{Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    // ---- Accepting requests ----

    [Fact]
    public async Task The_recipient_can_accept_a_pending_request()
    {
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Explorer), $"/api/social/follow-requests/{requestId}/accept");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredRequest(requestId).Status.Should().Be(FollowRequestStatus.Accepted);
        IsFollowing(SecondExplorer, Explorer).Should().BeTrue();
        IsFollowing(Explorer, SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public async Task An_accepted_request_cannot_be_accepted_again()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.AdministratorAcceptedRequestToSecondExplorer.Id;

        var response = await PostAsync(ClientFor(SecondExplorer), $"/api/social/follow-requests/{requestId}/accept");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
        StoredRequest(requestId).Status.Should().Be(FollowRequestStatus.Accepted);
    }

    [Fact]
    public async Task A_rejected_request_cannot_be_accepted()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.ExplorerRejectedRequestToSecondExplorer.Id;

        var response = await PostAsync(ClientFor(SecondExplorer), $"/api/social/follow-requests/{requestId}/accept");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
        IsFollowing(Explorer, SecondExplorer).Should().BeFalse();
    }

    [Fact]
    public async Task Only_the_recipient_can_accept_a_request()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Administrator), $"/api/social/follow-requests/{requestId}/accept");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_request_that_does_not_exist_cannot_be_accepted()
    {
        var before = GetStoredState();

        var response = await PostAsync(ClientFor(Explorer), $"/api/social/follow-requests/{Guid.NewGuid()}/accept");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    // ---- Rejecting requests ----

    [Fact]
    public async Task The_recipient_can_reject_a_pending_request()
    {
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Explorer), $"/api/social/follow-requests/{requestId}/reject");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredRequest(requestId).Status.Should().Be(FollowRequestStatus.Rejected);
        IsFollowing(SecondExplorer, Explorer).Should().BeFalse();
    }

    [Fact]
    public async Task An_accepted_request_cannot_be_rejected()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.AdministratorAcceptedRequestToSecondExplorer.Id;

        var response = await PostAsync(ClientFor(SecondExplorer), $"/api/social/follow-requests/{requestId}/reject");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
        IsFollowing(Administrator, SecondExplorer).Should().BeTrue();
    }

    [Fact]
    public async Task Only_the_recipient_can_reject_a_request()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Administrator), $"/api/social/follow-requests/{requestId}/reject");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_request_that_does_not_exist_cannot_be_rejected()
    {
        var before = GetStoredState();

        var response = await PostAsync(ClientFor(Explorer), $"/api/social/follow-requests/{Guid.NewGuid()}/reject");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    // ---- Accepting and following back ----

    [Fact]
    public async Task Accepting_and_following_back_makes_both_users_follow_each_other()
    {
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Explorer), $"/api/social/follow-requests/{requestId}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredRequest(requestId).Status.Should().Be(FollowRequestStatus.Accepted);
        IsFollowing(SecondExplorer, Explorer).Should().BeTrue();
        IsFollowing(Explorer, SecondExplorer).Should().BeTrue();
        using var context = Factory.CreateContext<SocialDbContext>();
        context.FollowRequests.Count(request =>
            request.SenderId == Explorer
            && request.RecipientId == SecondExplorer
            && request.Status == FollowRequestStatus.Accepted).Should().Be(1);
    }

    [Fact]
    public async Task Accepting_and_following_back_leaves_the_users_own_pending_request_to_the_sender_unchanged()
    {
        var ownRequestId = FollowSeed.ExplorerPendingRequestToThirdExplorer.Id;
        var ownRequestBefore = StoredRequest(ownRequestId);
        var requestsBefore = GetStoredState().Requests.Count;
        var requestId = FollowSeed.ThirdExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Explorer), $"/api/social/follow-requests/{requestId}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredRequest(requestId).Status.Should().Be(FollowRequestStatus.Accepted);
        IsFollowing(ThirdExplorer, Explorer).Should().BeTrue();
        IsFollowing(Explorer, ThirdExplorer).Should().BeFalse("the user's own request is still waiting for an answer");
        var ownRequestAfter = StoredRequest(ownRequestId);
        ownRequestAfter.Status.Should().Be(FollowRequestStatus.Pending);
        ownRequestAfter.RespondedAt.Should().Be(ownRequestBefore.RespondedAt);
        GetStoredState().Requests.Should().HaveCount(requestsBefore, "no second follow-back request may be created");
    }

    [Fact]
    public async Task Accepting_and_following_back_creates_no_new_follow_if_the_user_already_follows_the_sender()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.SecondExplorerPendingRequestToThirdExplorer.Id;

        var response = await PostAsync(ClientFor(ThirdExplorer), $"/api/social/follow-requests/{requestId}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        StoredRequest(requestId).Status.Should().Be(FollowRequestStatus.Accepted);
        IsFollowing(SecondExplorer, ThirdExplorer).Should().BeTrue();
        var after = GetStoredState();
        after.Requests.Should().HaveCount(before.Requests.Count, "no follow-back request is needed");
        after.Follows.Count(follow => follow.FollowerId == ThirdExplorer && follow.FollowedUserId == SecondExplorer)
            .Should().Be(1, "the existing follow must stay the only one");
        after.Follows.Should().HaveCount(before.Follows.Count + 1, "only the accepted request adds a follow");
    }

    [Fact]
    public async Task An_answered_request_cannot_be_accepted_and_followed_back()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.AdministratorAcceptedRequestToSecondExplorer.Id;

        var response = await PostAsync(ClientFor(SecondExplorer), $"/api/social/follow-requests/{requestId}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task Only_the_recipient_can_accept_and_follow_back()
    {
        var before = GetStoredState();
        var requestId = FollowSeed.SecondExplorerPendingRequestToExplorer.Id;

        var response = await PostAsync(ClientFor(Administrator), $"/api/social/follow-requests/{requestId}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_request_that_does_not_exist_cannot_be_accepted_and_followed_back()
    {
        var before = GetStoredState();

        var response = await PostAsync(
            ClientFor(Explorer), $"/api/social/follow-requests/{Guid.NewGuid()}/accept-and-follow-back");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    // ---- Removing followers ----

    [Fact]
    public async Task A_user_can_remove_a_follower()
    {
        var response = await DeleteAsync(ClientFor(SecondExplorer), $"/api/social/followers/{Administrator}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        IsFollowing(Administrator, SecondExplorer).Should().BeFalse();
        IsFollowing(SecondExplorer, Administrator).Should().BeTrue();
    }

    [Fact]
    public async Task A_user_cannot_remove_a_user_who_does_not_follow_them()
    {
        var before = GetStoredState();

        var response = await DeleteAsync(ClientFor(Administrator), $"/api/social/followers/{Explorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    [Fact]
    public async Task A_user_cannot_remove_a_user_who_only_sent_a_pending_request()
    {
        var before = GetStoredState();

        var response = await DeleteAsync(ClientFor(Explorer), $"/api/social/followers/{SecondExplorer}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        GetStoredState().Should().BeEquivalentTo(before);
    }

    // ---- Helpers ----

    private sealed record StoredState(
        List<(Guid Id, FollowRequestStatus Status, DateTime? RespondedAt)> Requests,
        List<(Guid FollowerId, Guid FollowedUserId)> Follows);

    private HttpClient ClientFor(Guid userId) =>
        Factory.CreateClientFor(userId, userId == Administrator ? "administrator" : "explorer");

    private static Task<HttpResponseMessage> SendRequestAsync(HttpClient client, Guid recipientId) =>
        client.PostAsync($"/api/social/users/{recipientId}/follow-requests", null);

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string url) => client.PostAsync(url, null);

    private static Task<HttpResponseMessage> DeleteAsync(HttpClient client, string url) => client.DeleteAsync(url);

    private FollowRequest StoredRequest(Guid requestId)
    {
        using var context = Factory.CreateContext<SocialDbContext>();
        return context.FollowRequests.Single(request => request.Id == requestId);
    }

    private bool IsFollowing(Guid followerId, Guid followedUserId)
    {
        using var context = Factory.CreateContext<SocialDbContext>();
        return context.Follows.Any(follow => follow.FollowerId == followerId && follow.FollowedUserId == followedUserId);
    }

    private StoredState GetStoredState()
    {
        using var context = Factory.CreateContext<SocialDbContext>();
        var requests = context.FollowRequests
            .Select(request => new { request.Id, request.Status, request.RespondedAt })
            .AsEnumerable()
            .Select(request => (request.Id, request.Status, request.RespondedAt))
            .ToList();
        var follows = context.Follows
            .Select(follow => new { follow.FollowerId, follow.FollowedUserId })
            .AsEnumerable()
            .Select(follow => (follow.FollowerId, follow.FollowedUserId))
            .ToList();

        return new StoredState(requests, follows);
    }
}
