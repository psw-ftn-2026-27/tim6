using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Social.Application.Clubs;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubReading;

public class ClubReadingQueryTests : BaseIntegrationTest
{
    public ClubReadingQueryTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Club_list_contains_clubs_of_different_owners()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs", JsonOptions);

        clubs!.Items.Should().Contain(club => club.Id == ClubSeed.HikingClub.Id);
        clubs.Items.Should().Contain(club => club.Id == ClubSeed.SecondExplorersClub.Id);
        clubs.TotalCount.Should().Be(ClubSeed.All.Length);
    }

    [Fact]
    public async Task Club_list_starts_with_the_newest_club()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var clubs = await client.GetFromJsonAsync<PageResult<ClubDto>>("/api/social/clubs", JsonOptions);

        clubs!.Items.First().Id.Should().Be(ClubSeed.All.MaxBy(club => club.CreatedAt)!.Id);
    }

    [Fact]
    public async Task Opened_club_shows_its_name_description_and_images()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var club = await client.GetFromJsonAsync<ClubDto>(
            $"/api/social/clubs/{ClubSeed.PhotographyClub.Id}", JsonOptions);

        club!.Id.Should().Be(ClubSeed.PhotographyClub.Id);
        club.Name.Should().Be(ClubSeed.PhotographyClub.Name);
        club.Description.Should().Be(ClubSeed.PhotographyClub.Description);
        club.Images.Should().Equal(ClubSeed.PhotographyClub.Images);
    }

    [Fact]
    public async Task Club_that_does_not_exist_cannot_be_opened()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/social/clubs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_user_cannot_list_clubs()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/social/clubs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_user_cannot_open_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
