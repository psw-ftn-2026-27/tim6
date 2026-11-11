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
}
