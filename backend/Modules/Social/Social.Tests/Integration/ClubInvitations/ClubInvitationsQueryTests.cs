using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubInvitations;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubInvitations;

public class ClubInvitationsQueryTests : BaseIntegrationTest
{
    public ClubInvitationsQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_sees_the_clubs_invitations_with_the_invitees_email_and_sending_date()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var club = ClubSeed.ClubWithDemoExplorersInvitation;

        var invitations = await client.GetFromJsonAsync<List<ClubInvitationDto>>(
            $"/api/social/clubs/{club.Id}/invitations", JsonOptions);

        var seededInvitation = club.Invitations.Single();
        var invitation = invitations!.Single(invitation => invitation.UserId == seededInvitation.UserId);
        invitation.UserEmail.Should().Be(BlogSeed.IdentityDemoExplorerEmail);
        invitation.SentAt.Should().BeCloseTo(seededInvitation.SentAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task Moderator_does_not_see_the_clubs_invitations()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/clubs/{ClubSeed.ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.Id}/invitations");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
