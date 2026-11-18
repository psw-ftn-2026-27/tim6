using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubMembership;
using Social.Domain.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubMembership;

public class ClubMembershipQueryTests : BaseIntegrationTest
{
    public ClubMembershipQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task User_with_a_pending_request_sees_that_the_request_is_pending()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var membership = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.ClubWithSecondExplorersJoinRequest.Id}/membership", JsonOptions);

        membership!.Status.Should().Be(ClubMembershipStatus.Pending);
    }

    [Fact]
    public async Task Club_member_sees_that_they_are_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var membership = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.HikingClub.Id}/membership", JsonOptions);

        membership!.Status.Should().Be(ClubMembershipStatus.Member);
    }

    [Fact]
    public async Task Founder_sees_that_they_are_a_member_with_the_owner_role()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var membership = await client.GetFromJsonAsync<ClubMembershipDto>(
            $"/api/social/clubs/{ClubSeed.HikingClub.Id}/membership", JsonOptions);

        membership!.Status.Should().Be(ClubMembershipStatus.Member);
        membership.Role.Should().Be(ClubRole.Owner);
    }
}
