using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Application.ExplorerPositioning;
using Shared.Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Shared.Positions.Tests.Integration.ExplorerPositioning;

public class ExplorerPositioningQueryTests : BaseIntegrationTest
{
    public ExplorerPositioningQueryTests(SharedPositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_their_own_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var position = await client.GetFromJsonAsync<CoordinatesDto>("/api/shared/positions/mine", JsonOptions);

        position!.Latitude.Should().Be(ExplorerPositionSeed.ExplorerAtTheFortress.Coordinates.Latitude);
        position.Longitude.Should().Be(ExplorerPositionSeed.ExplorerAtTheFortress.Coordinates.Longitude);
    }

    [Fact]
    public async Task Explorer_who_never_set_a_position_gets_no_position_rather_than_an_error()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/shared/positions/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Anonymous_user_cannot_read_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/shared/positions/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_read_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/shared/positions/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
