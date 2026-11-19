using FluentAssertions;
using Games.Domain.Challenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public class LocationChallengeTests
{
    private static List<HiddenPoint> CreateHiddenPoints(int count) =>
        Enumerable.Range(0, count)
            .Select(_ => new HiddenPoint("/api/shared/images/fasada.jpg", new Location(45.2553, 19.8449)))
            .ToList();

    private static LocationChallenge CreateChallenge() =>
        new("Skrivene fasade", 100, CreateHiddenPoints(1));

    private static LocationChallenge CreatePublishedChallenge()
    {
        var challenge = CreateChallenge();
        challenge.PlaceAt(new Location(45.2551, 19.8451));
        challenge.Publish();
        return challenge;
    }

    [Fact]
    public void New_location_challenge_is_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Location_challenge_without_a_name_is_rejected(string name)
    {
        var creation = () => new LocationChallenge(name, 100, CreateHiddenPoints(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_name_longer_than_two_hundred_characters_is_rejected()
    {
        var creation = () => new LocationChallenge(new string('a', 201), 100, CreateHiddenPoints(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_with_a_name_of_two_hundred_characters_is_created()
    {
        var name = new string('a', 200);

        var challenge = new LocationChallenge(name, 100, CreateHiddenPoints(1));

        challenge.Name.Should().Be(name);
    }

    [Fact]
    public void Location_challenge_without_hidden_points_is_rejected()
    {
        var creation = () => new LocationChallenge("Skrivene fasade", 100, CreateHiddenPoints(0));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_with_one_hidden_point_is_created()
    {
        var challenge = new LocationChallenge("Skrivene fasade", 100, CreateHiddenPoints(1));

        challenge.HiddenPoints.Should().HaveCount(1);
    }

    [Fact]
    public void Location_challenge_without_xp_is_rejected()
    {
        var creation = () => new LocationChallenge("Skrivene fasade", 0, CreateHiddenPoints(1));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Location_challenge_rewarding_one_xp_is_created()
    {
        var challenge = new LocationChallenge("Skrivene fasade", 1, CreateHiddenPoints(1));

        challenge.Xp.Should().Be(1);
    }

    [Fact]
    public void Published_location_challenge_cannot_be_edited()
    {
        var challenge = CreatePublishedChallenge();

        var edit = () => challenge.Edit("Skrivene fasade", 100, CreateHiddenPoints(1));

        edit.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", 200, 2)]
    [InlineData("Nove fasade", 0, 2)]
    [InlineData("Nove fasade", 200, 0)]
    public void Draft_edited_with_invalid_data_keeps_its_previous_data_and_hidden_points(string name, int xp,
        int hiddenPointCount)
    {
        var challenge = CreateChallenge();
        var hiddenPointsBefore = challenge.HiddenPoints.ToList();

        var edit = () => challenge.Edit(name, xp, CreateHiddenPoints(hiddenPointCount));

        edit.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Skrivene fasade");
        challenge.Xp.Should().Be(100);
        challenge.HiddenPoints.Should().Equal(hiddenPointsBefore);
    }
}
