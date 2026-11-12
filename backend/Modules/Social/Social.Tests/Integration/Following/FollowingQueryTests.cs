using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Following;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

public class FollowingQueryTests : BaseIntegrationTest
{
    public FollowingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Authors_own_blog_shows_no_follow_option()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var status = await client.GetFromJsonAsync<BlogFollowStatusDto>(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/follow-status", JsonOptions);

        status!.State.Should().Be(BlogFollowState.OwnBlog);
    }

    [Fact]
    public async Task Follow_status_for_an_author_without_any_relationship_is_follow()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var status = await client.GetFromJsonAsync<BlogFollowStatusDto>(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/follow-status", JsonOptions);

        status!.State.Should().Be(BlogFollowState.Follow);
    }

    [Fact]
    public async Task Sent_requests_list_shows_only_the_users_own_requests_with_statuses()
    {
        var expected = FollowRelationshipSeed.All
            .SelectMany(relationship => relationship.Requests)
            .Where(request => request.SenderId == WellKnownUsers.SecondExplorer);
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var requests = await client.GetFromJsonAsync<List<SentFollowRequestDto>>(
            "/api/social/follow-requests/sent", JsonOptions);

        requests!.Select(request => (request.Id, request.Status))
            .Should().BeEquivalentTo(expected.Select(request => (request.Id, request.Status)));
    }

    [Fact]
    public async Task Followed_users_list_shows_only_active_follows_not_request_history()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var followedUsers = await client.GetFromJsonAsync<List<FollowedUserDto>>(
            "/api/social/following", JsonOptions);

        var followedUserIds = followedUsers!.Select(user => user.UserId).ToList();
        followedUserIds.Should().Contain(WellKnownUsers.Administrator);
        followedUserIds.Should().NotContain(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Received_requests_list_shows_only_pending_requests_where_the_user_is_the_recipient()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var requests = await client.GetFromJsonAsync<List<ReceivedFollowRequestDto>>(
            "/api/social/follow-requests/received", JsonOptions);

        requests!.Select(request => request.SenderId).Should().Contain(WellKnownUsers.SecondExplorer);
        requests.Should().OnlyContain(request => request.SenderId != WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Followers_list_shows_only_users_who_currently_follow_the_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var followers = await client.GetFromJsonAsync<List<FollowerDto>>(
            "/api/social/followers", JsonOptions);

        var followerIds = followers!.Select(follower => follower.UserId).ToList();
        followerIds.Should().Contain(WellKnownUsers.SecondExplorer);
        followerIds.Should().NotContain(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Followers_list_flags_whether_the_user_already_follows_back()
    {
        var explorerClient = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var sendResponse = await explorerClient.PostAsync(
            $"/api/social/users/{WellKnownUsers.Administrator}/follow-requests", null);
        var requestId = await sendResponse.Content.ReadFromJsonAsync<Guid>();
        var administratorClient = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        await administratorClient.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);

        var followers = await administratorClient.GetFromJsonAsync<List<FollowerDto>>(
            "/api/social/followers", JsonOptions);

        followers!.Should().ContainSingle(follower => follower.UserId == WellKnownUsers.Explorer)
            .Which.IsFollowedBack.Should().BeFalse();
    }

    [Fact]
    public async Task Followers_list_flags_a_pending_request_the_user_sent_to_a_follower()
    {
        var explorerClient = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var sendResponse = await explorerClient.PostAsync(
            $"/api/social/users/{WellKnownUsers.SecondExplorer}/follow-requests", null);
        var requestId = await sendResponse.Content.ReadFromJsonAsync<Guid>();
        var secondExplorerClient = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        await secondExplorerClient.PostAsync($"/api/social/follow-requests/{requestId}/accept", null);

        var followers = await secondExplorerClient.GetFromJsonAsync<List<FollowerDto>>(
            "/api/social/followers", JsonOptions);

        var follower = followers!.Should().ContainSingle(candidate => candidate.UserId == WellKnownUsers.Explorer).Subject;
        follower.IsFollowedBack.Should().BeFalse();
        follower.HasPendingRequest.Should().BeTrue();
    }
}
