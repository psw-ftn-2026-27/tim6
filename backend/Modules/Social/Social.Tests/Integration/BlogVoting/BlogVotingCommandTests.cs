using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogVoting;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogVoting;

public class BlogVotingCommandTests : BaseIntegrationTest
{
    public BlogVotingCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_votes_for_a_published_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CastVoteDto(VoteSign.Up);

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.PublishedRiverbank.Id);
        stored.Votes.Should().ContainSingle(vote =>
            vote.UserId == WellKnownUsers.Administrator && vote.Sign == VoteSign.Up);
    }

    [Fact]
    public async Task Vote_for_an_archived_blog_is_rejected_and_not_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CastVoteDto(VoteSign.Up);

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.ArchivedLakeTrip.Id}/votes", request, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.ArchivedLakeTrip.Id);
        stored.Votes.Should().BeEmpty();
    }

    [Fact]
    public async Task Anonymous_user_cannot_vote_for_a_blog()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes", new CastVoteDto(VoteSign.Up), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
