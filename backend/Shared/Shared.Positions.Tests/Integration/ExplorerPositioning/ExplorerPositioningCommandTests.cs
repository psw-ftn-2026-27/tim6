using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Application.ExplorerPositioning;
using Shared.Infrastructure.Persistence;
using Shared.Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Shared.Positions.Tests.Integration.ExplorerPositioning;

public class ExplorerPositioningCommandTests : BaseIntegrationTest
{
    public ExplorerPositioningCommandTests(SharedPositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task First_position_is_stored_for_the_signed_in_explorer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = new CoordinatesDto(44.8231, 20.4502);
        using var arrangeContext = Factory.CreateContext<SharedDbContext>();
        var positionCountBefore = arrangeContext.ExplorerPositions.Count();

        var response = await client.PutAsJsonAsync("/api/shared/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.ExplorerPositions.Count().Should().Be(positionCountBefore + 1);
        var stored = assertContext.ExplorerPositions.Single(position => position.ExplorerId == WellKnownUsers.SecondExplorer);
        stored.Coordinates.Latitude.Should().Be(44.8231);
        stored.Coordinates.Longitude.Should().Be(20.4502);
    }

    [Fact]
    public async Task New_position_replaces_the_previous_one_and_the_explorer_keeps_a_single_row()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CoordinatesDto(45.2551, 19.8452);
        using var arrangeContext = Factory.CreateContext<SharedDbContext>();
        var positionCountBefore = arrangeContext.ExplorerPositions.Count();

        var response = await client.PutAsJsonAsync("/api/shared/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        assertContext.ExplorerPositions.Count().Should().Be(positionCountBefore);
        var stored = assertContext.ExplorerPositions.Single(position => position.ExplorerId == WellKnownUsers.Explorer);
        stored.Id.Should().Be(ExplorerPositionSeed.ExplorerAtTheFortress.Id);
        stored.Coordinates.Latitude.Should().Be(45.2551);
        stored.Coordinates.Longitude.Should().Be(19.8452);
    }

    [Fact]
    public async Task Position_outside_the_valid_range_is_rejected_and_the_previous_position_remains()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CoordinatesDto(90.1, 19.8452);

        var response = await client.PutAsJsonAsync("/api/shared/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SharedDbContext>();
        var stored = assertContext.ExplorerPositions.Single(position => position.ExplorerId == WellKnownUsers.Explorer);
        stored.Coordinates.Should().Be(ExplorerPositionSeed.ExplorerAtTheFortress.Coordinates);
    }

    [Fact]
    public async Task Anonymous_user_cannot_set_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync("/api/shared/positions/mine", new CoordinatesDto(45.2551, 19.8452));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_set_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PutAsJsonAsync("/api/shared/positions/mine", new CoordinatesDto(45.2551, 19.8452));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
