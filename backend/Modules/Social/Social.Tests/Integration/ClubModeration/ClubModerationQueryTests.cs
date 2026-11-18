using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubModeration;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubModeration;

public class ClubModerationQueryTests : BaseIntegrationTest
{
    public ClubModerationQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_sees_pending_requests_with_the_applicants_email_and_sending_date()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var club = ClubSeed.ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest;

        var requests = await client.GetFromJsonAsync<List<ClubJoinRequestDto>>(
            $"/api/social/clubs/{club.Id}/join-requests", JsonOptions);

        var seededRequest = club.JoinRequests.Single();
        var request = requests!.Single(request => request.UserId == seededRequest.UserId);
        request.UserEmail.Should().Be(BlogSeed.IdentityDemoExplorerEmail);
        request.SentAt.Should().BeCloseTo(seededRequest.SentAt, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public async Task User_who_is_neither_owner_nor_moderator_does_not_see_the_clubs_requests()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync(
            $"/api/social/clubs/{ClubSeed.ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest.Id}/join-requests");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
