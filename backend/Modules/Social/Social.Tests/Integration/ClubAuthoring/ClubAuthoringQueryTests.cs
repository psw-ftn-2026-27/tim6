using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubAuthoring;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubAuthoring;

public class ClubAuthoringQueryTests : BaseIntegrationTest
{
    public ClubAuthoringQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Owner_lists_only_their_own_clubs()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<List<ClubDto>>("/api/social/clubs/mine", JsonOptions);

        clubs.Should().HaveCount(ClubSeed.All.Count(club => club.OwnerId == WellKnownUsers.Explorer));
        clubs.Should().OnlyContain(club => club.OwnerId == WellKnownUsers.Explorer);
        clubs.Should().NotContain(club => club.Id == ClubSeed.SecondExplorersClub.Id);
    }
}
