using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringCommandTests : BaseIntegrationTest
{
    public KeyPointAuthoringCommandTests(ExplorationApiFactory factory) : base(factory) { }

    private static SaveKeyPointDto CreateKeyPointRequest(double lengthInKilometers) =>
        new("Dunavski park", "Park u centru grada.", "/api/shared/images/dunavski-park.jpg", "Tajna klupa.", new LocationDto(45.2550, 19.8490), true, lengthInKilometers);

    private static HttpRequestMessage CreateRemoveRequest(Guid tourId, Guid keyPointId, double lengthInKilometers) =>
        new(HttpMethod.Delete, $"/api/exploration/tours/{tourId}/key-points/{keyPointId}")
        {
            Content = JsonContent.Create(new RemoveKeyPointDto(lengthInKilometers))
        };

    [Fact]
    public async Task Author_adds_a_key_point_to_their_draft_tour_and_its_new_length_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = CreateKeyPointRequest(5.3);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Should().HaveCount(keyPointCountBefore + 1);
        var added = stored.KeyPoints.Single(keyPoint => keyPoint.Order == keyPointCountBefore + 1);
        added.Name.Should().Be("Dunavski park");
        added.Secret.Should().Be("Tajna klupa.");
        added.Location.Should().Be(new Location(45.2550, 19.8490));
        added.IsVisibleWithoutPurchase.Should().BeTrue();
        stored.Length.Should().Be(new TourLength(5.3));
    }

    [Fact]
    public async Task Author_cannot_add_a_key_point_to_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = CreateKeyPointRequest(5.3);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Should().HaveCount(keyPointCountBefore);
        stored.Length.Should().Be(TourSeed.PublishableRiverside.Length);
    }

    [Fact]
    public async Task Author_cannot_add_a_key_point_to_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = CreateKeyPointRequest(5.3);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id).KeyPoints.Count;

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.KeyPoints.Should().HaveCount(keyPointCountBefore);
        stored.Length.Should().Be(TourSeed.PublishedVineyards.Length);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_key_point()
    {
        var client = Factory.CreateClient();
        var request = CreateKeyPointRequest(5.3);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateKeyPointRequest(5.3);

        var response = await client.PostAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_changes_a_key_point_of_their_tour_and_its_new_length_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var keyPointId = TourSeed.PublishableRiverside.KeyPoints[0].Id;
        var request = CreateKeyPointRequest(4.8);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points/{keyPointId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        var changed = stored.KeyPoints.Single(keyPoint => keyPoint.Id == keyPointId);
        changed.Name.Should().Be("Dunavski park");
        changed.Description.Should().Be("Park u centru grada.");
        changed.Image.Should().Be("/api/shared/images/dunavski-park.jpg");
        changed.Secret.Should().Be("Tajna klupa.");
        changed.Location.Should().Be(new Location(45.2550, 19.8490));
        changed.IsVisibleWithoutPurchase.Should().BeTrue();
        changed.Order.Should().Be(TourSeed.PublishableRiverside.KeyPoints[0].Order);
        stored.Length.Should().Be(new TourLength(4.8));
    }

    [Fact]
    public async Task Author_cannot_change_a_key_point_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var keyPointId = TourSeed.PublishableRiverside.KeyPoints[0].Id;
        var request = CreateKeyPointRequest(4.8);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points/{keyPointId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Single(keyPoint => keyPoint.Id == keyPointId).Name.Should().Be(TourSeed.PublishableRiverside.KeyPoints[0].Name);
        stored.Length.Should().Be(TourSeed.PublishableRiverside.Length);
    }

    [Fact]
    public async Task Author_cannot_change_a_key_point_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = CreateKeyPointRequest(4.8);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Should().NotContain(keyPoint => keyPoint.Name == "Dunavski park");
        stored.Length.Should().Be(TourSeed.PublishableRiverside.Length);
    }

    [Fact]
    public async Task Anonymous_user_cannot_change_a_key_point()
    {
        var client = Factory.CreateClient();
        var keyPointId = TourSeed.PublishableRiverside.KeyPoints[0].Id;
        var request = CreateKeyPointRequest(4.8);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points/{keyPointId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_change_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var keyPointId = TourSeed.PublishableRiverside.KeyPoints[0].Id;
        var request = CreateKeyPointRequest(4.8);

        var response = await client.PutAsJsonAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points/{keyPointId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Author_removes_a_key_point_of_their_tour_and_the_remaining_points_and_new_length_are_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var removedId = TourSeed.PublishableRiverside.KeyPoints[1].Id;
        var request = CreateRemoveRequest(TourSeed.PublishableRiverside.Id, removedId, 2.9);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var keyPointCountBefore = arrangeContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id).KeyPoints.Count;

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Should().HaveCount(keyPointCountBefore - 1);
        stored.KeyPoints.Should().NotContain(keyPoint => keyPoint.Id == removedId);
        stored.KeyPoints.Select(keyPoint => keyPoint.Order).Should().BeEquivalentTo(Enumerable.Range(1, keyPointCountBefore - 1));
        stored.Length.Should().Be(new TourLength(2.9));
    }

    [Fact]
    public async Task Author_cannot_remove_a_key_point_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var keyPointId = TourSeed.PublishableRiverside.KeyPoints[1].Id;
        var request = CreateRemoveRequest(TourSeed.PublishableRiverside.Id, keyPointId, 2.9);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishableRiverside.Id);
        stored.KeyPoints.Should().Contain(keyPoint => keyPoint.Id == keyPointId);
        stored.Length.Should().Be(TourSeed.PublishableRiverside.Length);
    }

    [Fact]
    public async Task Author_cannot_remove_a_key_point_of_a_published_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var keyPointId = TourSeed.PublishedVineyards.KeyPoints[0].Id;
        var request = CreateRemoveRequest(TourSeed.PublishedVineyards.Id, keyPointId, 0);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.Tours.Single(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        stored.KeyPoints.Should().Contain(keyPoint => keyPoint.Id == keyPointId);
        stored.Length.Should().Be(TourSeed.PublishedVineyards.Length);
    }

    [Fact]
    public async Task Anonymous_user_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClient();
        var request = CreateRemoveRequest(TourSeed.PublishableRiverside.Id, TourSeed.PublishableRiverside.KeyPoints[1].Id, 2.9);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateRemoveRequest(TourSeed.PublishableRiverside.Id, TourSeed.PublishableRiverside.KeyPoints[1].Id, 2.9);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
