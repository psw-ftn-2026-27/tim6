using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourTests
{
    private static readonly string LongDescription = new('o', 100);
    private static readonly TourPrice FreePrice = new(true, null);

    private static Tour CreateTour(string description) =>
        new(WellKnownUsers.Explorer, "Šetnja tvrđavom", description, TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));

    private static void AddKeyPoint(Tour tour, TourLength length) =>
        tour.AddKeyPoint("Sahat kula", "Sat na tvrđavi.", "/api/shared/images/sahat-kula.jpg", null, new Location(45.25, 19.86), false, length);

    private static void AddTwoKeyPoints(Tour tour)
    {
        AddKeyPoint(tour, new TourLength(0));
        AddKeyPoint(tour, new TourLength(1.5));
    }

    private static Tour CreatePublishedTour()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddTwoKeyPoints(tour);
        tour.Publish(FreePrice);
        return tour;
    }

    private static Tour CreateArchivedTour()
    {
        var tour = CreatePublishedTour();
        tour.Archive();
        return tour;
    }

    [Fact]
    public void New_tour_starts_as_a_draft()
    {
        var tour = CreateTour(LongDescription);

        tour.Status.Should().Be(TourStatus.Draft);
        tour.PublishedAt.Should().BeNull();
        tour.TransportTimes.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_name(string name)
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, name, "Opis ture.", TourDifficulty.Easy, ["istorija"], new RecommendedGroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_tour_requires_a_description(string description)
    {
        var creation = () => CreateTour(description);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_tags()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, [], new RecommendedGroupSize(2, 6));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_tour_requires_a_recommended_group_size()
    {
        var creation = () => new Tour(WellKnownUsers.Explorer, "Šetnja tvrđavom", "Opis ture.", TourDifficulty.Easy, ["istorija"], null!);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_has_one_time_per_transport_mode()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, 90);

        addition.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    public void Transport_time_must_be_positive(int minutes)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddTransportTime(TransportMode.Walking, minutes);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_is_published_when_all_rules_are_met()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Bicycle, 45);
        AddTwoKeyPoints(tour);

        tour.Publish(FreePrice);

        tour.Status.Should().Be(TourStatus.Published);
        tour.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Published_tour_has_a_product_in_the_offer()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddTwoKeyPoints(tour);

        tour.Publish(FreePrice);

        tour.HasOfferProduct.Should().BeTrue();
    }

    [Fact]
    public void Tour_with_a_short_description_cannot_be_published()
    {
        var tour = CreateTour("Kratak opis.");
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddTwoKeyPoints(tour);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_transport_time_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        AddTwoKeyPoints(tour);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_without_a_chosen_price_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddTwoKeyPoints(tour);

        var publishing = () => tour.Publish(null);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_cannot_be_published_again()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Car, 30);
        AddTwoKeyPoints(tour);
        tour.Publish(FreePrice);

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_tour_is_archived_with_an_archiving_time()
    {
        var tour = CreatePublishedTour();

        tour.Archive();

        tour.Status.Should().Be(TourStatus.Archived);
        tour.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public void Draft_tour_cannot_be_archived()
    {
        var tour = CreateTour(LongDescription);

        var archiving = () => tour.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_is_activated_as_published_without_an_archiving_time_and_with_its_original_publishing_time()
    {
        var tour = CreateArchivedTour();
        var originalPublishedAt = tour.PublishedAt;

        tour.Activate();

        tour.Status.Should().Be(TourStatus.Published);
        tour.ArchivedAt.Should().BeNull();
        tour.PublishedAt.Should().Be(originalPublishedAt);
    }

    [Fact]
    public void Published_tour_cannot_be_activated()
    {
        var tour = CreatePublishedTour();

        var activation = () => tour.Activate();

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_get_a_transport_time()
    {
        var tour = CreateArchivedTour();

        var addition = () => tour.AddTransportTime(TransportMode.Bicycle, 45);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_tour_cannot_be_published()
    {
        var tour = CreateArchivedTour();

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Tour_with_fewer_than_two_key_points_cannot_be_published()
    {
        var tour = CreateTour(LongDescription);
        tour.AddTransportTime(TransportMode.Walking, 120);
        AddKeyPoint(tour, new TourLength(0));

        var publishing = () => tour.Publish(FreePrice);

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Remaining_key_points_are_renumbered_without_gaps_after_a_middle_one_is_removed()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPoint(tour, new TourLength(0));
        AddKeyPoint(tour, new TourLength(1.5));
        AddKeyPoint(tour, new TourLength(3));
        var middle = tour.KeyPoints.Single(keyPoint => keyPoint.Order == 2);

        tour.RemoveKeyPoint(middle.Id, new TourLength(2));

        tour.KeyPoints.Select(keyPoint => keyPoint.Order).Should().BeEquivalentTo([1, 2]);
    }

    [Fact]
    public void Tour_with_at_least_two_key_points_keeps_the_length_received_with_the_change()
    {
        var tour = CreateTour(LongDescription);
        AddKeyPoint(tour, new TourLength(0));

        AddKeyPoint(tour, new TourLength(2.4));

        tour.Length.Should().Be(new TourLength(2.4));
    }

    [Fact]
    public void Tour_with_one_key_point_has_a_length_of_zero_whatever_length_it_receives()
    {
        var tour = CreateTour(LongDescription);

        AddKeyPoint(tour, new TourLength(5));

        tour.Length.Should().Be(new TourLength(0));
    }

    [Fact]
    public void Key_point_cannot_be_added_to_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();

        var addition = () => AddKeyPoint(tour, new TourLength(3));

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void Key_point_cannot_be_changed_on_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();
        var keyPoint = tour.KeyPoints[0];

        var change = () => tour.UpdateKeyPoint(keyPoint.Id, "Novi naziv", "Novi opis.", "/api/shared/images/nova.jpg", null, new Location(45.26, 19.87), true, new TourLength(1.5));

        change.Should().Throw<DomainException>();
    }

    [Fact]
    public void Key_point_cannot_be_removed_from_a_tour_that_is_not_a_draft()
    {
        var tour = CreatePublishedTour();
        var keyPoint = tour.KeyPoints[0];

        var removal = () => tour.RemoveKeyPoint(keyPoint.Id, new TourLength(0));

        removal.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Sat na tvrđavi.", "/api/shared/images/sahat-kula.jpg")]
    [InlineData("Sahat kula", "   ", "/api/shared/images/sahat-kula.jpg")]
    [InlineData("Sahat kula", "Sat na tvrđavi.", "")]
    public void Key_point_requires_a_name_a_description_and_an_image(string name, string description, string image)
    {
        var tour = CreateTour(LongDescription);

        var addition = () => tour.AddKeyPoint(name, description, image, null, new Location(45.25, 19.86), false, new TourLength(0));

        addition.Should().Throw<DomainException>();
    }
}
