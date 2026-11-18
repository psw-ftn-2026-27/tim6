using System.Net;
using FluentAssertions;
using Shared.Tests;
using Social.Domain.Clubs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubModeration;

public class ClubModerationCommandTests : BaseIntegrationTest
{
    private static readonly Guid ClubId = ClubSeed.ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest.Id;
    private static readonly Guid ApplicantId = BlogSeed.IdentityDemoExplorer;

    public ClubModerationCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Accepted_applicant_becomes_a_plain_member_and_the_request_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/clubs/{ClubId}/join-requests/{ApplicantId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubId);
        stored.Members.Should().ContainSingle(member => member.UserId == ApplicantId && member.Role == ClubRole.Member);
        stored.JoinRequests.Should().NotContain(request => request.UserId == ApplicantId);
    }

    [Fact]
    public async Task Rejected_request_is_removed_and_the_applicant_is_not_a_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/clubs/{ClubId}/join-requests/{ApplicantId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubId);
        stored.JoinRequests.Should().NotContain(request => request.UserId == ApplicantId);
        stored.Members.Should().NotContain(member => member.UserId == ApplicantId);
    }

    [Fact]
    public async Task Plain_member_cannot_accept_a_request_and_the_request_remains_without_a_new_member()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var memberCountBefore = arrangeContext.Clubs.Single(club => club.Id == ClubId).Members.Count;

        var response = await client.PostAsync($"/api/social/clubs/{ClubId}/join-requests/{ApplicantId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubId);
        stored.JoinRequests.Should().ContainSingle(request => request.UserId == ApplicantId);
        stored.Members.Should().HaveCount(memberCountBefore);
    }

    [Fact]
    public async Task Request_to_a_club_that_does_not_exist_cannot_be_accepted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var unknownClubId = Guid.NewGuid();

        var response = await client.PostAsync($"/api/social/clubs/{unknownClubId}/join-requests/{ApplicantId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == unknownClubId).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_accept_a_join_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/clubs/{ClubId}/join-requests/{ApplicantId}/accept", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_reject_a_join_request()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/social/clubs/{ClubId}/join-requests/{ApplicantId}/reject", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
