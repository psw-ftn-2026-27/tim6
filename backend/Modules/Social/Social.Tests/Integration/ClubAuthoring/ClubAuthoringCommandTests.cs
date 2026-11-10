using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.ClubAuthoring;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.ClubAuthoring;

public class ClubAuthoringCommandTests : BaseIntegrationTest
{
    public ClubAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Founded_club_is_stored_with_its_founder_as_owner()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja.", ["https://example.com/muzej.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clubId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore + 1);
        var stored = assertContext.Clubs.Single(club => club.Id == clubId);
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
        stored.Name.Should().Be("Ljubitelji muzeja");
        stored.Description.Should().Be("Zajednički obilasci muzeja.");
        stored.Images.Should().ContainSingle().Which.Should().Be("https://example.com/muzej.jpg");
    }

    [Fact]
    public async Task Club_without_an_image_is_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja.", []);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_found_a_club()
    {
        var client = Factory.CreateClient();
        var request = new CreateClubDto("Ljubitelji muzeja", "Zajednički obilasci muzeja.", ["https://example.com/muzej.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/clubs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Updated_club_is_stored_with_new_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari Vojvodine", "Šetnje po celoj Vojvodini.", ["https://example.com/vojvodina.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.HikingClub.Id);
        stored.Name.Should().Be("Planinari Vojvodine");
        stored.Description.Should().Be("Šetnje po celoj Vojvodini.");
        stored.Images.Should().ContainSingle().Which.Should().Be("https://example.com/vojvodina.jpg");
        stored.OwnerId.Should().Be(WellKnownUsers.Explorer);
    }

    [Fact]
    public async Task Unknown_club_cannot_be_updated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Planinari Vojvodine", "Šetnje po celoj Vojvodini.", ["https://example.com/vojvodina.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Name == "Planinari Vojvodine").Should().BeFalse();
    }

    [Fact]
    public async Task Another_users_club_is_not_updated()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateClubDto("Preuzet klub", "Tuđa izmena.", ["https://example.com/preuzet.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.SecondExplorersClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Clubs.Single(club => club.Id == ClubSeed.SecondExplorersClub.Id);
        stored.Name.Should().Be(ClubSeed.SecondExplorersClub.Name);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_club()
    {
        var client = Factory.CreateClient();
        var request = new UpdateClubDto("Planinari Vojvodine", "Šetnje po celoj Vojvodini.", ["https://example.com/vojvodina.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Deleted_club_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore - 1);
        assertContext.Clubs.Any(club => club.Id == ClubSeed.HikingClub.Id).Should().BeFalse();
    }

    [Fact]
    public async Task Unknown_club_cannot_be_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var clubCountBefore = arrangeContext.Clubs.Count();

        var response = await client.DeleteAsync($"/api/social/clubs/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Count().Should().Be(clubCountBefore);
    }

    [Fact]
    public async Task Another_users_club_is_not_deleted()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.SecondExplorersClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Clubs.Any(club => club.Id == ClubSeed.SecondExplorersClub.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_club()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/social/clubs/{ClubSeed.HikingClub.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
