using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.Following;
using Social.Domain.Following;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.Following;

public class FollowingQueryTests : BaseIntegrationTest
{
    public FollowingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Follow_status_is_follow_when_the_users_have_no_relationship()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var status = await GetAsync<FollowStatusDto>(
            client, $"/api/social/users/{WellKnownUsers.Explorer}/follow-status");

        status.Status.Should().Be(FollowStatus.Follow);
    }

    [Fact]
    public async Task Follow_status_is_pending_when_the_user_has_a_pending_request_to_the_other_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var status = await GetAsync<FollowStatusDto>(
            client, $"/api/social/users/{WellKnownUsers.Explorer}/follow-status");

        status.Status.Should().Be(FollowStatus.Pending);
    }

    [Fact]
    public async Task Follow_status_is_following_when_the_user_follows_the_other_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var status = await GetAsync<FollowStatusDto>(
            client, $"/api/social/users/{WellKnownUsers.SecondExplorer}/follow-status");

        status.Status.Should().Be(FollowStatus.Following);
    }

    [Fact]
    public async Task Sent_requests_list_shows_only_the_users_own_requests_with_statuses()
    {
        var expected = FollowSeed.All
            .OfType<FollowRequest>()
            .Where(request => request.SenderId == WellKnownUsers.SecondExplorer);
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var requests = await GetAsync<List<SentFollowRequestDto>>(client, "/api/social/follow-requests/sent");

        requests.Select(request => (request.Id, request.Status))
            .Should().BeEquivalentTo(expected.Select(request => (request.Id, request.Status)));
    }

    [Fact]
    public async Task Followed_users_exclude_users_with_only_a_pending_request()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var followedUsers = await GetAsync<List<FollowedUserDto>>(client, "/api/social/following");

        var followedUserIds = followedUsers.Select(user => user.UserId).ToList();
        followedUserIds.Should().Contain(WellKnownUsers.Administrator);
        followedUserIds.Should().NotContain(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Received_requests_list_contains_the_pending_requests_addressed_to_the_user()
    {
        var expectedRequestIds = FollowSeed.All
            .OfType<FollowRequest>()
            .Where(request => request.RecipientId == WellKnownUsers.Explorer && request.IsPending)
            .Select(request => request.Id);
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var requests = await GetAsync<List<ReceivedFollowRequestDto>>(client, "/api/social/follow-requests/received");

        requests.Select(request => request.Id).Should().BeEquivalentTo(expectedRequestIds);
    }

    [Fact]
    public async Task Received_requests_list_excludes_requests_that_were_already_answered()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var requests = await GetAsync<List<ReceivedFollowRequestDto>>(client, "/api/social/follow-requests/received");

        requests.Should().BeEmpty();
    }

    [Fact]
    public async Task Followers_list_shows_only_users_who_currently_follow_the_user()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var followers = await GetAsync<List<FollowerDto>>(client, "/api/social/followers");

        followers.Select(follower => follower.UserId).Should().BeEquivalentTo([WellKnownUsers.SecondExplorer]);
    }

    [Fact]
    public async Task Followers_list_flags_a_follower_the_user_follows_back()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var followers = await GetAsync<List<FollowerDto>>(client, "/api/social/followers");

        followers.Should().ContainSingle(follower => follower.UserId == WellKnownUsers.SecondExplorer)
            .Which.IsFollowedBack.Should().BeTrue();
    }

    [Fact]
    public async Task Followers_list_is_empty_for_a_user_whose_only_incoming_request_is_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var followers = await GetAsync<List<FollowerDto>>(client, "/api/social/followers");

        followers.Should().BeEmpty();
    }

    [Fact]
    public async Task Followers_list_flags_a_follower_the_user_does_not_follow_back()
    {
        var client = Factory.CreateClientFor(FollowSeed.ThirdExplorer, "explorer");

        var followers = await GetAsync<List<FollowerDto>>(client, "/api/social/followers");

        followers.Should().ContainSingle(follower => follower.UserId == WellKnownUsers.Administrator)
            .Which.IsFollowedBack.Should().BeFalse();
    }

    private static async Task<T> GetAsync<T>(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<T>(JsonOptions))!;
    }
}
