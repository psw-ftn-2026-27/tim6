using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringQueryTests : BaseIntegrationTest
{
    public KeyPointAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_gets_the_key_points_of_their_tour_in_order_with_the_stored_length()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var result = await client.GetFromJsonAsync<TourKeyPointsDto>($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points", JsonOptions);

        result!.Status.Should().Be(TourSeed.PublishableRiverside.Status);
        result.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(
            TourSeed.PublishableRiverside.KeyPoints.OrderBy(keyPoint => keyPoint.Order).Select(keyPoint => keyPoint.Id));
        result.KeyPoints.Select(keyPoint => keyPoint.Order).Should().BeInAscendingOrder();
        result.LengthInKilometers.Should().Be(TourSeed.PublishableRiverside.Length.Kilometers);
    }

    [Fact]
    public async Task Author_does_not_get_the_key_points_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.PublishableRiverside.Id}/key-points");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
