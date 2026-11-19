using FluentAssertions;
using Games.Domain.Challenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public class SocialChallengeTests
{
    private static DailyTimeWindow CreateTimeWindow() => new(new TimeOnly(18, 0), new TimeOnly(20, 0));

    private static SocialChallenge CreateChallenge(int requiredPeople = 5, int rangeInMeters = 50) =>
        new("Okupljanje na trgu", CreateTimeWindow(), requiredPeople, rangeInMeters, 100, "Svi ste se okupili!");

    private static SocialChallenge CreateLocatedChallenge()
    {
        var challenge = CreateChallenge();
        challenge.PlaceAt(new Location(45.2551, 19.8451));
        return challenge;
    }

    private static SocialChallenge CreatePublishedChallenge()
    {
        var challenge = CreateLocatedChallenge();
        challenge.Publish();
        return challenge;
    }

    private static readonly Location AtChallenge = new(45.2551, 19.8451);
    private static readonly Location FarFromChallenge = new(45.2561, 19.8451);
    private static readonly DateTime WindowOpening = new(2026, 11, 19, 18, 0, 0);
    private static readonly DateTime WindowClosing = new(2026, 11, 19, 20, 0, 0);

    private static SocialChallenge CreatePublishedChallengeFor(int requiredPeople)
    {
        var challenge = CreateChallenge(requiredPeople);
        challenge.PlaceAt(AtChallenge);
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
        var creation = () => new SocialChallenge(name, CreateTimeWindow(), 5, 50, 100, exitMessage);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Social_challenge_with_a_name_of_two_hundred_characters_is_created()
    {
        var name = new string('a', 200);

        var challenge = new SocialChallenge(name, CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!");

        challenge.Name.Should().Be(name);
    }

    [Fact]
    public void Social_challenge_name_longer_than_two_hundred_characters_is_rejected()
    {
        var creation = () => new SocialChallenge(new string('a', 201), CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!");

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
    public void Social_challenge_rewarding_one_xp_is_created()
    {
        var challenge = new SocialChallenge("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 1, "Svi ste se okupili!");

        challenge.Xp.Should().Be(1);
    }

    [Fact]
    public void Draft_cannot_be_edited_with_invalid_data()
    {
        var challenge = CreateChallenge();

        var edit = () => challenge.Edit("Okupljanje na trgu", CreateTimeWindow(), 31, 50, 100, "Svi ste se okupili!");

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
    public void Published_social_challenge_cannot_be_edited()
    {
        var challenge = CreatePublishedChallenge();

        var edit = () => challenge.Edit("Okupljanje na trgu", CreateTimeWindow(), 5, 50, 100, "Svi ste se okupili!");

        edit.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_is_placed_at_a_location()
    {
        var challenge = CreateChallenge();

        challenge.PlaceAt(new Location(44.8125, 20.4612));

        challenge.Location.Should().Be(new Location(44.8125, 20.4612));
    }

    [Fact]
    public void Draft_loses_its_location()
    {
        var challenge = CreateLocatedChallenge();

        challenge.RemoveLocation();

        challenge.Location.Should().BeNull();
    }

    [Fact]
    public void Published_social_challenge_cannot_be_moved()
    {
        var challenge = CreatePublishedChallenge();

        var placing = () => challenge.PlaceAt(new Location(44.8125, 20.4612));

        placing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_social_challenge_cannot_lose_its_location()
    {
        var challenge = CreatePublishedChallenge();

        var removal = () => challenge.RemoveLocation();

        removal.Should().Throw<DomainException>();
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

    [Theory]
    [InlineData(17, 59)]
    [InlineData(20, 0)]
    [InlineData(23, 30)]
    public void Social_challenge_is_not_activated_outside_its_daily_time_window(int hour, int minute)
    {
        var challenge = CreatePublishedChallengeFor(2);

        var activation = () => challenge.Activate(Guid.NewGuid(), AtChallenge, new DateTime(2026, 11, 19, hour, minute, 0));

        activation.Should().Throw<DomainException>();
        challenge.ActiveRun.Should().BeNull();
    }

    [Fact]
    public void Tourist_who_activates_while_a_run_is_active_joins_it_instead_of_starting_a_new_one()
    {
        var challenge = CreatePublishedChallengeFor(5);
        var firstTourist = Guid.NewGuid();
        var secondTourist = Guid.NewGuid();
        challenge.Activate(firstTourist, AtChallenge, WindowOpening);
        var activeRun = challenge.ActiveRun;

        challenge.Activate(secondTourist, AtChallenge, WindowOpening.AddSeconds(10));

        challenge.ActiveRun.Should().BeSameAs(activeRun);
        challenge.ActiveRun!.Participants.Select(participant => participant.TouristId)
            .Should().BeEquivalentTo([firstTourist, secondTourist]);
    }

    [Fact]
    public void Check_removes_the_participant_out_of_range_and_the_one_silent_for_thirty_seconds()
    {
        var challenge = CreatePublishedChallengeFor(5);
        var silentTourist = Guid.NewGuid();
        var recentTourist = Guid.NewGuid();
        var leavingTourist = Guid.NewGuid();
        challenge.Activate(silentTourist, AtChallenge, WindowOpening);
        challenge.Activate(recentTourist, AtChallenge, WindowOpening.AddSeconds(20));
        challenge.Activate(leavingTourist, AtChallenge, WindowOpening.AddSeconds(25));

        challenge.CheckPosition(leavingTourist, FarFromChallenge, WindowOpening.AddSeconds(30));

        challenge.ActiveRun!.Participants.Select(participant => participant.TouristId)
            .Should().BeEquivalentTo([recentTourist]);
    }

    [Fact]
    public void Check_after_the_window_ends_closes_the_run_and_nobody_earns_xp()
    {
        var challenge = CreatePublishedChallengeFor(2);
        var firstTourist = Guid.NewGuid();
        var secondTourist = Guid.NewGuid();
        challenge.Activate(firstTourist, AtChallenge, WindowClosing.AddSeconds(-20));
        challenge.Activate(secondTourist, AtChallenge, WindowClosing.AddSeconds(-10));

        var solvers = challenge.CheckPosition(firstTourist, AtChallenge, WindowClosing);

        solvers.Should().BeEmpty();
        challenge.ActiveRun.Should().BeNull();
    }

    [Fact]
    public void Check_with_enough_active_participants_solves_the_challenge_for_them_only_and_closes_the_run()
    {
        var challenge = CreatePublishedChallengeFor(2);
        var silentTourist = Guid.NewGuid();
        var firstTourist = Guid.NewGuid();
        var secondTourist = Guid.NewGuid();
        challenge.Activate(silentTourist, AtChallenge, WindowOpening);
        challenge.Activate(firstTourist, AtChallenge, WindowOpening.AddSeconds(20));
        challenge.Activate(secondTourist, AtChallenge, WindowOpening.AddSeconds(30));

        var solvers = challenge.CheckPosition(secondTourist, AtChallenge, WindowOpening.AddSeconds(40));

        solvers.Should().BeEquivalentTo([firstTourist, secondTourist]);
        challenge.ActiveRun.Should().BeNull();
    }

    [Fact]
    public void Run_state_does_not_count_a_participant_silent_for_thirty_seconds_and_shows_them_as_left()
    {
        var challenge = CreatePublishedChallengeFor(5);
        var silentTourist = Guid.NewGuid();
        challenge.Activate(silentTourist, AtChallenge, WindowOpening);
        challenge.Activate(Guid.NewGuid(), AtChallenge, WindowOpening.AddSeconds(20));

        var state = challenge.RunStateFor(silentTourist, WindowOpening.AddSeconds(30));

        state.Should().Be(new RunState(RunStatus.Left, 1));
    }

    [Fact]
    public void Run_state_after_the_window_ends_shows_the_run_as_closed_rather_than_left()
    {
        var challenge = CreatePublishedChallengeFor(5);
        var tourist = Guid.NewGuid();
        challenge.Activate(tourist, AtChallenge, WindowClosing.AddSeconds(-10));

        var state = challenge.RunStateFor(tourist, WindowClosing);

        state.Should().Be(new RunState(RunStatus.Closed, 0));
    }
}
