using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourStarterTests
{
    private static readonly Location Position = new(45.2530, 19.8620);

    private readonly TourStarter _tourStarter = new();

    private static Tour CreateDraftTour()
    {
        var tour = new Tour(WellKnownUsers.SecondExplorer, "Šetnja tvrđavom", new string('o', 100), TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));
        tour.AddTransportTime(TransportMode.Walking, 120);
        tour.AddKeyPoint("Sahat kula", "Sat na tvrđavi.", "/api/shared/images/sahat-kula.jpg", null, new Location(45.25, 19.86), false, new TourLength(0));
        tour.AddKeyPoint("Vidikovac", "Pogled na Dunav.", "/api/shared/images/vidikovac.jpg", null, new Location(45.26, 19.87), false, new TourLength(1.5));
        return tour;
    }

    private static Tour CreatePublishedTour()
    {
        var tour = CreateDraftTour();
        tour.Publish(new TourPrice(true, null));
        return tour;
    }

    [Fact]
    public void Tourist_without_an_active_session_starts_a_purchased_published_tour()
    {
        var tour = CreatePublishedTour();

        var execution = _tourStarter.Start(tour, null, true, WellKnownUsers.Explorer, Position);

        execution.Status.Should().Be(TourExecutionStatus.Active);
        execution.TourId.Should().Be(tour.Id);
        execution.TouristId.Should().Be(WellKnownUsers.Explorer);
        execution.StartPosition.Should().Be(Position);
        execution.StartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        execution.EndedAt.Should().BeNull();
    }

    [Fact]
    public void Purchased_archived_tour_can_be_started()
    {
        var tour = CreatePublishedTour();
        tour.Archive();

        var execution = _tourStarter.Start(tour, null, true, WellKnownUsers.Explorer, Position);

        execution.Status.Should().Be(TourExecutionStatus.Active);
        execution.TourId.Should().Be(tour.Id);
    }

    [Fact]
    public void Draft_tour_cannot_be_started()
    {
        var tour = CreateDraftTour();

        var start = () => _tourStarter.Start(tour, null, true, WellKnownUsers.Explorer, Position);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_that_is_not_purchased_cannot_be_started()
    {
        var tour = CreatePublishedTour();

        var start = () => _tourStarter.Start(tour, null, false, WellKnownUsers.Explorer, Position);

        start.Should().Throw<DomainException>();
    }

    [Fact]
    public void Starting_the_same_tour_again_continues_the_active_session()
    {
        var tour = CreatePublishedTour();
        var activeExecution = new TourExecution(WellKnownUsers.Explorer, tour.Id, Position);

        var execution = _tourStarter.Start(tour, activeExecution, true, WellKnownUsers.Explorer, new Location(45.2600, 19.8700));

        execution.Should().BeSameAs(activeExecution);
    }

    [Fact]
    public void Another_tour_cannot_be_started_while_one_is_active()
    {
        var tour = CreatePublishedTour();
        var activeExecution = new TourExecution(WellKnownUsers.Explorer, Guid.NewGuid(), Position);

        var start = () => _tourStarter.Start(tour, activeExecution, true, WellKnownUsers.Explorer, Position);

        start.Should().Throw<DomainException>();
    }
}
