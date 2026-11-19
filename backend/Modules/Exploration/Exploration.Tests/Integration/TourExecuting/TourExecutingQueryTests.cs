using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecuting;
using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
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

        var execution = await client.GetFromJsonAsync<TourExecutionDto>($"/api/exploration/tour-executions/current/{TourSeed.PublishedVineyards.Id}", JsonOptions);

        execution!.Id.Should().Be(TourExecutionSeed.ExplorersActiveVineyards.Id);
        execution.TourId.Should().Be(TourSeed.PublishedVineyards.Id);
        execution.Status.Should().Be(TourExecutionStatus.Active);
        execution.NextKeyPoint!.Name.Should().Be(firstKeyPoint.Name);
        execution.NextKeyPoint.Description.Should().Be(firstKeyPoint.Description);
        execution.NextKeyPoint.Image.Should().Be(firstKeyPoint.Image);
        execution.NextKeyPoint.Location.Latitude.Should().Be(firstKeyPoint.Location.Latitude);
        execution.NextKeyPoint.Location.Longitude.Should().Be(firstKeyPoint.Location.Longitude);
        execution.LastReachedKeyPoint.Should().BeNull();
    }

    [Fact]
    public async Task Tourist_without_their_own_active_session_sees_nothing_while_another_tourist_takes_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tour-executions/current/{TourSeed.PublishedVineyards.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Tourist_with_an_active_session_gets_the_next_key_point_and_the_last_reached_key_point_with_its_secret()
    {
        var client = Factory.CreateClientFor(TourExecutionSeed.ThirdExplorer, "explorer");
        var keyPoints = TourSeed.PublishedFortressUndergroundWithPreview.KeyPoints.OrderBy(keyPoint => keyPoint.Order).ToList();

        var execution = await client.GetFromJsonAsync<TourExecutionDto>($"/api/exploration/tour-executions/current/{TourSeed.PublishedFortressUndergroundWithPreview.Id}", JsonOptions);

        execution!.Id.Should().Be(TourExecutionSeed.ThirdExplorersActiveFortressUndergroundAfterTwoKeyPoints.Id);
        execution.Status.Should().Be(TourExecutionStatus.Active);
        execution.NextKeyPoint!.Name.Should().Be(keyPoints[2].Name);
        execution.LastReachedKeyPoint!.Name.Should().Be(keyPoints[1].Name);
        execution.LastReachedKeyPoint.Secret.Should().Be(keyPoints[1].Secret);
    }

    [Fact]
    public async Task Tourist_whose_latest_session_is_completed_gets_the_completed_session()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var execution = await client.GetFromJsonAsync<TourExecutionDto>($"/api/exploration/tour-executions/current/{TourSeed.PublishedMonasteries.Id}", JsonOptions);

        execution!.Id.Should().Be(TourExecutionSeed.SecondExplorersCompletedMonasteries.Id);
        execution.Status.Should().Be(TourExecutionStatus.Completed);
        execution.NextKeyPoint.Should().BeNull();
    }

    [Fact]
    public async Task Tourist_gets_secrets_completed_in_an_abandoned_session_but_not_of_uncompleted_key_points_or_other_tourists_sessions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedFortressUndergroundWithPreview;
        var completedIds = TourExecutionSeed.ExplorersAbandonedFortressUndergroundAfterTheFirstKeyPoint.CompletedKeyPoints
            .Select(completed => completed.KeyPointId)
            .ToList();
        var expected = tour.KeyPoints
            .Where(keyPoint => completedIds.Contains(keyPoint.Id))
            .OrderBy(keyPoint => keyPoint.Order)
            .Select(keyPoint => new UnlockedKeyPointDto(keyPoint.Name, new LocationDto(keyPoint.Location.Latitude, keyPoint.Location.Longitude), keyPoint.Secret))
            .ToList();
        var keyPointCompletedOnlyByAnotherTourist = tour.KeyPoints[1];

        var secrets = await client.GetFromJsonAsync<List<UnlockedKeyPointDto>>($"/api/exploration/tour-executions/unlocked-secrets/{tour.Id}", JsonOptions);

        secrets.Should().Equal(expected);
        secrets.Should().NotContain(secret => secret.Name == keyPointCompletedOnlyByAnotherTourist.Name);
    }
}
