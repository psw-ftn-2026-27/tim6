using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourProgressTrackerTests
{
    private const double MetersPerDegreeOfLatitude = 111_194.93;

    private static readonly Location FirstKeyPointLocation = new(45.2528, 19.8628);
    private static readonly Location SecondKeyPointLocation = new(45.2515, 19.8650);

    private readonly TourProgressTracker _progressTracker = new();

    private static Tour CreatePublishedTour()
    {
        var tour = new Tour(WellKnownUsers.SecondExplorer, "Podzemlje Petrovaradina", new string('o', 100), TourDifficulty.Moderate, ["istorija"], new RecommendedGroupSize(2, 8));
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.AddKeyPoint("Sahat kula", "Kula sa satom.", "/api/shared/images/sahat-kula.jpg", "Kazaljke su obrnute.", FirstKeyPointLocation, true, new TourLength(0));
        tour.AddKeyPoint("Podzemne galerije", "Ulaz u galerije.", "/api/shared/images/podzemne-galerije.jpg", "Iza treće kapije je prolaz.", SecondKeyPointLocation, false, new TourLength(1.2));
        tour.Publish(new TourPrice(true, null));
        return tour;
    }

    private static TourExecution CreateActiveExecution(Tour tour) =>
        new(WellKnownUsers.Explorer, tour.Id, new Location(45.2530, 19.8620));

    [Theory]
    [InlineData(49.5, true)]
    [InlineData(50.5, false)]
    public void Tourist_within_fifty_meters_completes_the_next_key_point_and_every_report_records_activity(double metersNorth, bool isCompleted)
    {
        var tour = CreatePublishedTour();
        var execution = CreateActiveExecution(tour);
        var position = new Location(FirstKeyPointLocation.Latitude + metersNorth / MetersPerDegreeOfLatitude, FirstKeyPointLocation.Longitude);

        _progressTracker.ReportPosition(execution, tour, position);

        execution.CompletedKeyPoints.Any(completed => completed.KeyPointId == tour.KeyPoints[0].Id).Should().Be(isCompleted);
        execution.CompletedKeyPoints.Should().HaveCount(isCompleted ? 1 : 0);
        execution.LastActivityAt.Should().BeAfter(execution.StartedAt);
    }

    [Fact]
    public void Being_near_a_later_key_point_completes_nothing_while_the_next_one_is_not_completed()
    {
        var tour = CreatePublishedTour();
        var execution = CreateActiveExecution(tour);

        _progressTracker.ReportPosition(execution, tour, SecondKeyPointLocation);

        execution.CompletedKeyPoints.Should().BeEmpty();
    }

    [Fact]
    public void Completing_the_last_key_point_completes_the_session_with_an_end_time()
    {
        var tour = CreatePublishedTour();
        var execution = CreateActiveExecution(tour);
        _progressTracker.ReportPosition(execution, tour, FirstKeyPointLocation);

        _progressTracker.ReportPosition(execution, tour, SecondKeyPointLocation);

        execution.CompletedKeyPoints.Select(completed => completed.KeyPointId).Should().Equal(tour.KeyPoints[0].Id, tour.KeyPoints[1].Id);
        execution.Status.Should().Be(TourExecutionStatus.Completed);
        execution.EndedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Session_that_is_not_active_does_not_accept_a_position()
    {
        var tour = CreatePublishedTour();
        var execution = CreateActiveExecution(tour);
        execution.Abandon();

        var report = () => _progressTracker.ReportPosition(execution, tour, FirstKeyPointLocation);

        report.Should().Throw<DomainException>();
    }
}
