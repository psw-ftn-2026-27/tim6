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
        assertContext.TourExecutions.Should().NotContain(execution =>
            execution.TouristId == WellKnownUsers.SecondExplorer && execution.TourId == TourSeed.PublishedFortressUndergroundWithPreview.Id);
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

    [Fact]
    public async Task Tourist_near_the_next_key_point_reports_a_position_and_the_key_point_is_completed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var firstKeyPoint = TourSeed.PublishedVineyards.KeyPoints.OrderBy(keyPoint => keyPoint.Order).First();
        var position = new LocationDto(firstKeyPoint.Location.Latitude, firstKeyPoint.Location.Longitude);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var lastActivityBefore = arrangeContext.TourExecutions
            .Single(execution => execution.Id == TourExecutionSeed.ExplorersActiveVineyards.Id).LastActivityAt;

        var response = await client.PostAsJsonAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/position", position);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == TourExecutionSeed.ExplorersActiveVineyards.Id);
        var completed = stored.CompletedKeyPoints.Should().ContainSingle().Subject;
        completed.KeyPointId.Should().Be(firstKeyPoint.Id);
        completed.ReachedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        stored.LastActivityAt.Should().BeAfter(lastActivityBefore);
    }

    [Fact]
    public async Task Tourist_without_an_active_session_cannot_report_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var firstKeyPoint = TourSeed.PublishedVineyards.KeyPoints.OrderBy(keyPoint => keyPoint.Order).First();
        var position = new LocationDto(firstKeyPoint.Location.Latitude, firstKeyPoint.Location.Longitude);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var lastActivitiesBefore = arrangeContext.TourExecutions
            .Where(execution => execution.TourId == TourSeed.PublishedVineyards.Id)
            .ToDictionary(execution => execution.Id, execution => execution.LastActivityAt);

        var response = await client.PostAsJsonAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/position", position);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var storedExecutions = assertContext.TourExecutions
            .Where(execution => execution.TourId == TourSeed.PublishedVineyards.Id)
            .ToList();
        storedExecutions.Should().OnlyContain(execution => execution.CompletedKeyPoints.Count == 0);
        storedExecutions.ToDictionary(execution => execution.Id, execution => execution.LastActivityAt)
            .Should().Equal(lastActivitiesBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_report_a_position()
    {
        var client = Factory.CreateClient();
        var position = new LocationDto(45.2030, 19.9330);

        var response = await client.PostAsJsonAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/position", position);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_report_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var position = new LocationDto(45.2030, 19.9330);

        var response = await client.PostAsJsonAsync($"/api/exploration/tour-executions/active/{TourSeed.PublishedVineyards.Id}/position", position);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
