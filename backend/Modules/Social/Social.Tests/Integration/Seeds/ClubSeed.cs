using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club HikingClub = new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend šetnje stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"], new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
    public static readonly Club PhotographyClub = new(WellKnownUsers.Explorer, "Foto šetnje", "Fotografisanje gradskih znamenitosti.", ["https://example.com/foto1.jpg", "https://example.com/foto2.jpg"], new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc));
    public static readonly Club SecondExplorersClub = new(WellKnownUsers.SecondExplorer, "Tuđi klub", "Klub drugog osnivača.", ["https://example.com/tudji-klub.jpg"], new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc));

    public static Club[] All => [HikingClub, PhotographyClub, SecondExplorersClub];
}
