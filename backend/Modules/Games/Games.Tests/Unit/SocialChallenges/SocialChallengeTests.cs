using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static DailyTimeWindow CreateTimeWindow() => new(new TimeOnly(18, 0), new TimeOnly(20, 0));

    private static SocialChallenge CreateChallenge(int requiredPeople = 5, int rangeInMeters = 50) =>
        new("Okupljanje na trgu", CreateTimeWindow(), requiredPeople, rangeInMeters, 100, "Svi ste se okupili!");

    [Fact]
    public void New_social_challenge_is_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(ChallengeStatus.Draft);
    }

    [Theory]
    [InlineData("", "Svi ste se okupili!")]
    [InlineData("   ", "Svi ste se okupili!")]
    [InlineData("Okupljanje na trgu", "")]
    [InlineData("Okupljanje na trgu", "   ")]
    public void New_social_challenge_requires_a_name_and_an_exit_message(string name, string exitMessage)
    {
        var creation = () => new SocialChallenge(name, CreateTimeWindow(), 5, 50, 100, exitMessage);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(31)]
    public void Social_challenge_requires_between_two_and_thirty_people(int requiredPeople)
    {
        var creation = () => CreateChallenge(requiredPeople: requiredPeople);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(30)]
    public void Social_challenge_for_two_or_thirty_people_is_created(int requiredPeople)
    {
        var challenge = CreateChallenge(requiredPeople: requiredPeople);

        challenge.RequiredPeople.Should().Be(requiredPeople);
    }

    [Theory]
    [InlineData(19)]
    [InlineData(301)]
    public void Social_challenge_range_is_between_twenty_and_three_hundred_meters(int rangeInMeters)
    {
        var creation = () => CreateChallenge(rangeInMeters: rangeInMeters);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(20)]
    [InlineData(300)]
    public void Social_challenge_with_a_range_of_twenty_or_three_hundred_meters_is_created(int rangeInMeters)
    {
        var challenge = CreateChallenge(rangeInMeters: rangeInMeters);

        challenge.RangeInMeters.Should().Be(rangeInMeters);
    }

    [Fact]
    public void Social_challenge_without_xp_cannot_be_created()
    {
        var creation = () => new SocialChallenge("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 0, "Svi ste se okupili!");

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_cannot_be_edited_with_invalid_data()
    {
        var challenge = CreateChallenge();

        var edit = () => challenge.Edit("Okupljanje na trgu", CreateTimeWindow(), 31, 50, 100, "Svi ste se okupili!");

        edit.Should().Throw<DomainException>();
    }
}
