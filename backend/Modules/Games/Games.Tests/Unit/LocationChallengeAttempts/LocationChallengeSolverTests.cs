using FluentAssertions;
using Games.Domain.Challenges;
using Games.Domain.LocationChallengeAttempts;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Unit.LocationChallengeAttempts;

public class LocationChallengeSolverTests
{
    private const double MetersPerDegreeOfLatitude = Math.PI * 6_371_000 / 180;

    private static readonly Location Start = new(45.2551, 19.8547);
    private static readonly Location NearPoint = new(45.2554, 19.8547);
    private static readonly Location FarPoint = new(45.2545, 19.8556);

    private readonly LocationChallengeSolver _solver = new();

    private static Location MetersNorthOf(Location location, double meters) =>
        new(location.Latitude + meters / MetersPerDegreeOfLatitude, location.Longitude);

    private static LocationChallenge CreatePublishedChallenge()
    {
        var challenge = new LocationChallenge("Detalji sa keja", 100,
        [
            new HiddenPoint("/api/shared/images/kej-1.jpg", NearPoint),
            new HiddenPoint("/api/shared/images/kej-2.jpg", FarPoint)
        ]);
        challenge.PlaceAt(Start);
        challenge.Publish();
        return challenge;
    }

    private LocationChallengeAttempt ActivateAtStart(LocationChallenge challenge) =>
        _solver.Activate(challenge, null, WellKnownUsers.Explorer, Start, 40);

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    public void Explorer_within_fifty_meters_of_the_start_location_activates_the_challenge(double meters)
    {
        var challenge = CreatePublishedChallenge();

        var attempt = _solver.Activate(challenge, null, WellKnownUsers.Explorer, MetersNorthOf(Start, meters), 40);

        attempt.LocationChallengeId.Should().Be(challenge.Id);
        attempt.ExplorerId.Should().Be(WellKnownUsers.Explorer);
        attempt.ActivatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        attempt.CompletedAt.Should().BeNull();
        attempt.RevealedHiddenPointIds.Should().BeEmpty();
    }

    [Fact]
    public void Explorer_slightly_farther_than_fifty_meters_from_the_start_location_cannot_activate_the_challenge()
    {
        var challenge = CreatePublishedChallenge();

        var activation = () => _solver.Activate(challenge, null, WellKnownUsers.Explorer, MetersNorthOf(Start, 50.1), 40);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_that_is_not_published_cannot_be_activated()
    {
        var challenge = new LocationChallenge("Detalji sa keja", 100, [new HiddenPoint("/api/shared/images/kej-1.jpg", NearPoint)]);
        challenge.PlaceAt(Start);

        var activation = () => _solver.Activate(challenge, null, WellKnownUsers.Explorer, Start, 40);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Challenge_whose_attempt_is_completed_cannot_be_activated_again()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);
        _solver.CheckLocation(challenge, attempt, NearPoint);
        _solver.CheckLocation(challenge, attempt, FarPoint);

        var activation = () => _solver.Activate(challenge, attempt, WellKnownUsers.Explorer, Start, 40);

        activation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Extra_radius_from_zero_to_one_hundred_meters_is_accepted(double extraMeters)
    {
        var challenge = CreatePublishedChallenge();

        var attempt = _solver.Activate(challenge, null, WellKnownUsers.Explorer, Start, extraMeters);

        attempt.RadiusInMeters.Should().Be(Start.DistanceTo(FarPoint) + extraMeters);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Extra_radius_outside_zero_to_one_hundred_meters_is_rejected(double extraMeters)
    {
        var challenge = CreatePublishedChallenge();

        var activation = () => _solver.Activate(challenge, null, WellKnownUsers.Explorer, Start, extraMeters);

        activation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Radius_of_a_new_attempt_is_the_distance_to_the_farthest_hidden_point_plus_the_extra_radius()
    {
        var challenge = CreatePublishedChallenge();

        var attempt = _solver.Activate(challenge, null, WellKnownUsers.Explorer, Start, 37.5);

        attempt.RadiusInMeters.Should().Be(Start.DistanceTo(FarPoint) + 37.5);
    }

    [Fact]
    public void Activating_an_open_attempt_again_continues_it_with_the_same_circle_revealed_points_and_activation_time()
    {
        var challenge = CreatePublishedChallenge();
        var openAttempt = ActivateAtStart(challenge);
        _solver.CheckLocation(challenge, openAttempt, NearPoint);
        var radiusBefore = openAttempt.RadiusInMeters;
        var activatedAtBefore = openAttempt.ActivatedAt;
        var revealedBefore = openAttempt.RevealedHiddenPointIds.ToList();

        var attempt = _solver.Activate(challenge, openAttempt, WellKnownUsers.Explorer, Start, 90);

        attempt.Should().BeSameAs(openAttempt);
        attempt.RadiusInMeters.Should().Be(radiusBefore);
        attempt.ActivatedAt.Should().Be(activatedAtBefore);
        attempt.RevealedHiddenPointIds.Should().Equal(revealedBefore);
    }

    [Fact]
    public void Open_attempt_is_continued_far_from_the_start_location()
    {
        var challenge = CreatePublishedChallenge();
        var openAttempt = ActivateAtStart(challenge);

        var attempt = _solver.Activate(challenge, openAttempt, WellKnownUsers.Explorer, MetersNorthOf(Start, 500), 40);

        attempt.Should().BeSameAs(openAttempt);
    }

    [Fact]
    public void Check_exactly_five_meters_from_an_unrevealed_hidden_point_reveals_it()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);

        _solver.CheckLocation(challenge, attempt, MetersNorthOf(NearPoint, 5));

        attempt.RevealedHiddenPointIds.Should().Equal(challenge.HiddenPoints[0].Id);
        attempt.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Check_slightly_farther_than_five_meters_from_every_unrevealed_hidden_point_is_rejected()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);

        var check = () => _solver.CheckLocation(challenge, attempt, MetersNorthOf(NearPoint, 5.1));

        check.Should().Throw<DomainException>();
        attempt.RevealedHiddenPointIds.Should().BeEmpty();
    }

    [Fact]
    public void Check_at_an_already_revealed_hidden_point_is_rejected_and_does_not_reveal_it_again()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);
        _solver.CheckLocation(challenge, attempt, NearPoint);

        var check = () => _solver.CheckLocation(challenge, attempt, NearPoint);

        check.Should().Throw<DomainException>();
        attempt.RevealedHiddenPointIds.Should().Equal(challenge.HiddenPoints[0].Id);
    }

    [Fact]
    public void Check_on_a_challenge_that_is_no_longer_published_is_rejected()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);
        challenge.Retire();

        var check = () => _solver.CheckLocation(challenge, attempt, NearPoint);

        check.Should().Throw<DomainException>();
        attempt.RevealedHiddenPointIds.Should().BeEmpty();
    }

    [Fact]
    public void Revealing_the_last_hidden_point_completes_the_attempt()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);
        _solver.CheckLocation(challenge, attempt, NearPoint);

        _solver.CheckLocation(challenge, attempt, FarPoint);

        attempt.RevealedHiddenPointIds.Should().BeEquivalentTo(challenge.HiddenPoints.Select(point => point.Id));
        attempt.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Check_on_a_completed_attempt_is_rejected_and_leaves_it_completed()
    {
        var challenge = CreatePublishedChallenge();
        var attempt = ActivateAtStart(challenge);
        _solver.CheckLocation(challenge, attempt, NearPoint);
        _solver.CheckLocation(challenge, attempt, FarPoint);
        var completedAtBefore = attempt.CompletedAt;

        var check = () => _solver.CheckLocation(challenge, attempt, FarPoint);

        check.Should().Throw<DomainException>();
        attempt.CompletedAt.Should().Be(completedAtBefore);
    }
}
