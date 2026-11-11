using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Unit.TourPreferences;

public class TourPreferenceTests
{
    private static TourPreference CreatePreference() =>
        new(WellKnownUsers.Explorer, [TourDifficulty.Easy], TransportMode.Walking, ["istorija"], false);

    [Fact]
    public void Preference_requires_at_least_one_difficulty()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [], TransportMode.Walking, ["istorija"], false);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preference_cannot_repeat_a_difficulty()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [TourDifficulty.Hard, TourDifficulty.Hard], TransportMode.Walking, ["istorija"], false);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preference_cannot_repeat_a_tag()
    {
        var creation = () => new TourPreference(WellKnownUsers.Explorer, [TourDifficulty.Easy], TransportMode.Walking, ["istorija", "istorija"], false);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Preference_without_tags_is_allowed()
    {
        var preference = new TourPreference(WellKnownUsers.Explorer, [TourDifficulty.Easy], TransportMode.Bicycle, [], false);

        preference.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Changed_preference_still_requires_at_least_one_difficulty()
    {
        var preference = CreatePreference();

        var update = () => preference.Update([], TransportMode.Car, ["priroda"], false);

        update.Should().Throw<DomainException>();
    }
}
