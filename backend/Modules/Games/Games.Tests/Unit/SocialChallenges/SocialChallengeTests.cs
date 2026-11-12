using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static DailyTimeWindow CreateTimeWindow() => new(new TimeOnly(18, 0), new TimeOnly(20, 0));

    private static SocialChallenge CreateChallenge(int requiredPeople = 5, int rangeInMeters = 50) =>
        new("Okupljanje na trgu", CreateTimeWindow(), requiredPeople, rangeInMeters, 100, "Svi ste se okupili!", null);

    private static SocialChallenge CreateLocatedChallenge() =>
        new("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!", new Location(45.2551, 19.8451));

    private static SocialChallenge CreatePublishedChallenge()
    {
        var challenge = CreateLocatedChallenge();
        challenge.Publish();
        return challenge;
    }

    private static SocialChallenge CreateRetiredChallenge()
    {
        var challenge = CreatePublishedChallenge();
        challenge.Retire();
        return challenge;
    }

    private static SocialChallenge CreateArchivedChallenge()
    {
        var challenge = CreateRetiredChallenge();
        challenge.Archive();
        return challenge;
    }

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
        var creation = () => new SocialChallenge(name, CreateTimeWindow(), 5, 50, 100, exitMessage, null);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Social_challenge_with_a_name_of_two_hundred_characters_is_created()
    {
        var name = new string('a', 200);

        var challenge = new SocialChallenge(name, CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!", null);

        challenge.Name.Should().Be(name);
    }

    [Fact]
    public void Social_challenge_name_longer_than_two_hundred_characters_is_rejected()
    {
        var creation = () => new SocialChallenge(new string('a', 201), CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!", null);

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
        var creation = () => new SocialChallenge("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 0, "Svi ste se okupili!", null);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Social_challenge_rewarding_one_xp_is_created()
    {
        var challenge = new SocialChallenge("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 1, "Svi ste se okupili!", null);

        challenge.Xp.Should().Be(1);
    }

    [Fact]
    public void Draft_cannot_be_edited_with_invalid_data()
    {
        var challenge = CreateChallenge();

        var edit = () => challenge.Edit("Okupljanje na trgu", CreateTimeWindow(), 31, 50, 100, "Svi ste se okupili!", null);

        edit.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_without_a_location_cannot_be_published()
    {
        var challenge = CreateChallenge();

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_with_a_location_is_published()
    {
        var challenge = CreateLocatedChallenge();

        challenge.Publish();

        challenge.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Retired_social_challenge_is_published_again()
    {
        var challenge = CreateRetiredChallenge();

        challenge.Publish();

        challenge.Status.Should().Be(ChallengeStatus.Published);
    }

    [Fact]
    public void Published_social_challenge_is_retired()
    {
        var challenge = CreatePublishedChallenge();

        challenge.Retire();

        challenge.Status.Should().Be(ChallengeStatus.Retired);
    }

    [Fact]
    public void Retired_social_challenge_is_archived()
    {
        var challenge = CreateRetiredChallenge();

        challenge.Archive();

        challenge.Status.Should().Be(ChallengeStatus.Archived);
    }

    [Fact]
    public void Archived_social_challenge_cannot_be_published()
    {
        var challenge = CreateArchivedChallenge();

        var publishing = () => challenge.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_cannot_be_retired()
    {
        var challenge = CreateLocatedChallenge();

        var retiring = () => challenge.Retire();

        retiring.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_social_challenge_cannot_be_archived()
    {
        var challenge = CreatePublishedChallenge();

        var archiving = () => challenge.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_social_challenge_cannot_be_edited_nor_moved()
    {
        var challenge = CreatePublishedChallenge();

        var edit = () => challenge.Edit("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!", new Location(44.8125, 20.4612));

        edit.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_social_challenge_cannot_be_deleted()
    {
        var challenge = CreatePublishedChallenge();

        var deletion = () => challenge.Delete();

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Retired_social_challenge_cannot_be_deleted()
    {
        var challenge = CreateRetiredChallenge();

        var deletion = () => challenge.Delete();

        deletion.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_social_challenge_can_be_deleted()
    {
        var challenge = CreateArchivedChallenge();

        var deletion = () => challenge.Delete();

        deletion.Should().NotThrow();
    }
}
