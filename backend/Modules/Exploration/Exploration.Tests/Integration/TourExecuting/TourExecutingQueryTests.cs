using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecuting;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecuting;

public class TourExecutingQueryTests : BaseIntegrationTest
{
    public TourExecutingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_sees_their_active_session_with_the_first_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var firstKeyPoint = TourSeed.PublishedVineyards.KeyPoints.OrderBy(keyPoint => keyPoint.Order).First();

        var execution = await client.GetFromJsonAsync<TourExecutionDto>($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}", JsonOptions);

        execution!.Id.Should().Be(TourExecutionSeed.ExplorersActiveVineyards.Id);
        execution.TourId.Should().Be(TourSeed.PublishedVineyards.Id);
        execution.NextKeyPoint.Name.Should().Be(firstKeyPoint.Name);
        execution.NextKeyPoint.Description.Should().Be(firstKeyPoint.Description);
        execution.NextKeyPoint.Image.Should().Be(firstKeyPoint.Image);
        execution.NextKeyPoint.Location.Latitude.Should().Be(firstKeyPoint.Location.Latitude);
        execution.NextKeyPoint.Location.Longitude.Should().Be(firstKeyPoint.Location.Longitude);
    }

    [Fact]
    public async Task Tourist_without_their_own_active_session_sees_nothing_while_another_tourist_takes_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
