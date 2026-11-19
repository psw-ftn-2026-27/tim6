using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecuting;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecuting;

public class TourExecutingCommandTests : BaseIntegrationTest
{
    public TourExecutingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    private static StartTourDto CreateStartRequest(Guid tourId) =>
        new(tourId, new LocationDto(45.1200, 19.9400));

    [Fact]
    public async Task Tourist_starts_a_purchased_tour_and_gets_an_active_session()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = CreateStartRequest(TourSeed.PublishedMonasteries.Id);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore + 1);
        var stored = assertContext.TourExecutions.Single(execution =>
            execution.TouristId == WellKnownUsers.SecondExplorer && execution.Status == TourExecutionStatus.Active);
        stored.TourId.Should().Be(TourSeed.PublishedMonasteries.Id);
        stored.StartPosition.Latitude.Should().Be(request.Position.Latitude);
        stored.StartPosition.Longitude.Should().Be(request.Position.Longitude);
    }

    [Fact]
    public async Task Tourist_who_abandoned_a_tour_starts_it_again_in_a_new_session()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = CreateStartRequest(TourSeed.PublishedVineyards.Id);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore + 1);
        var stored = assertContext.TourExecutions.Single(execution =>
            execution.TouristId == WellKnownUsers.SecondExplorer && execution.Status == TourExecutionStatus.Active);
        stored.TourId.Should().Be(TourSeed.PublishedVineyards.Id);
        stored.Id.Should().NotBe(TourExecutionSeed.SecondExplorersAbandonedVineyards.Id);
        assertContext.TourExecutions.Single(execution => execution.Id == TourExecutionSeed.SecondExplorersAbandonedVineyards.Id)
            .Status.Should().Be(TourExecutionStatus.Abandoned);
    }

    [Fact]
    public async Task Tourist_cannot_start_a_tour_they_have_not_purchased()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var request = CreateStartRequest(TourSeed.PublishedFortressUndergroundWithPreview.Id);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
        assertContext.TourExecutions.Should().NotContain(execution => execution.TourId == TourSeed.PublishedFortressUndergroundWithPreview.Id);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_tour()
    {
        var client = Factory.CreateClient();
        var request = CreateStartRequest(TourSeed.PublishedMonasteries.Id);

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = CreateStartRequest(TourSeed.PublishedMonasteries.Id);

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_abandons_their_active_session()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == TourExecutionSeed.ExplorersActiveVineyards.Id);
        stored.Status.Should().Be(TourExecutionStatus.Abandoned);
        stored.EndedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Tourist_without_an_active_session_cannot_abandon_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var othersSession = assertContext.TourExecutions.Single(execution => execution.Id == TourExecutionSeed.ExplorersActiveVineyards.Id);
        othersSession.Status.Should().Be(TourExecutionStatus.Active);
        othersSession.EndedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/abandon", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
