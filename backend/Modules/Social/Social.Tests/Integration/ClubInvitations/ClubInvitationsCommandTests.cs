using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubInvitations;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubInvitations;

public class ClubInvitationsCommandTests : BaseIntegrationTest
{
    private static readonly Guid InvitingClubId = ClubSeed.ClubWithDemoExplorersInvitation.Id;
    private static readonly Guid InviteeId = BlogSeed.IdentityDemoExplorer;

    public ClubInvitationsCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Invitation_by_email_is_stored_for_the_user_with_that_email()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/social/clubs/{ClubSeed.HikingClub.Id}/invitations", new InviteDto(BlogSeed.IdentityDemoExplorerEmail));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikingClub.Id);
        stored.Invitations.Should().ContainSingle(invitation => invitation.UserId == BlogSeed.IdentityDemoExplorer);
    }

    [Fact]
    public async Task Invitation_to_an_email_without_a_user_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var invitationCountBefore = arrangeContext.Clubs
            .Single(club => club.Id == ClubSeed.HikingClub.Id)
            .Invitations.Count;

        var response = await client.PostAsJsonAsync(
            $"/api/social/clubs/{ClubSeed.HikingClub.Id}/invitations", new InviteDto("nobody@explorer.dev"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikingClub.Id);
        stored.Invitations.Should().HaveCount(invitationCountBefore);
    }

    [Fact]
    public async Task Invitation_to_a_club_member_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var clubId = ClubSeed.ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.Id;
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var invitationCountBefore = arrangeContext.Clubs.Single(club => club.Id == clubId).Invitations.Count;

        var response = await client.PostAsJsonAsync(
            $"/api/social/clubs/{clubId}/invitations", new InviteDto(BlogSeed.IdentityDemoExplorerEmail));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.Invitations.Should().HaveCount(invitationCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_invite_a_user_to_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/social/clubs/{ClubSeed.HikingClub.Id}/invitations", new InviteDto(BlogSeed.IdentityDemoExplorerEmail));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Withdrawn_invitation_is_removed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{InvitingClubId}/invitations/{InviteeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == InvitingClubId);
        stored.Invitations.Should().NotContain(invitation => invitation.UserId == InviteeId);
    }

    [Fact]
    public async Task Invitation_to_a_club_that_does_not_exist_cannot_be_withdrawn()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var unknownClubId = Guid.NewGuid();

        var response = await client.DeleteAsync($"/api/social/clubs/{unknownClubId}/invitations/{InviteeId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == unknownClubId).Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_user_cannot_withdraw_an_invitation()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{InvitingClubId}/invitations/{InviteeId}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
