using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourPreferenceSeed
{
    public static readonly TourPreference ExplorersPreference = new(WellKnownUsers.Explorer, [TourDifficulty.Easy, TourDifficulty.Moderate], TransportMode.Walking, ["istorija", "priroda"]);

    public static TourPreference[] All => [ExplorersPreference];
}
