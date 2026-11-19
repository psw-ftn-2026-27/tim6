using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogVoting;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogVoting;

public class BlogVotingQueryTests : BaseIntegrationTest
{
    public BlogVotingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_gets_their_stored_vote()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var vote = await client.GetFromJsonAsync<MyVoteDto>(
            $"/api/social/blogs/{BlogSeed.VotedCastleTour.Id}/votes/mine", JsonOptions);

        vote!.Sign.Should().Be(
            BlogSeed.VotedCastleTour.Votes.Single(seeded => seeded.UserId == WellKnownUsers.SecondExplorer).Sign);
    }

    [Fact]
    public async Task User_who_has_not_voted_for_a_blog_gets_no_vote()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var vote = await client.GetFromJsonAsync<MyVoteDto>(
            $"/api/social/blogs/{BlogSeed.PublishedRiverbank.Id}/votes/mine", JsonOptions);

        vote!.Sign.Should().BeNull();
    }
}
