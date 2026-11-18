using Shared.Tests;
using Social.Domain.Clubs;

namespace Social.Tests.Integration.Seeds;

internal static class ClubSeed
{
    public static readonly Club HikingClub = new(WellKnownUsers.Explorer, "Planinari Fruške gore", "Vikend šetnje stazama Fruške gore.", ["https://example.com/fruska-gora.jpg"], new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc));
    public static readonly Club PhotographyClub = new(WellKnownUsers.Explorer, "Foto šetnje", "Fotografisanje gradskih znamenitosti.", ["https://example.com/foto1.jpg", "https://example.com/foto2.jpg"], new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Utc));
    public static readonly Club SecondExplorersClub = new(WellKnownUsers.SecondExplorer, "Tuđi klub", "Klub drugog osnivača.", ["https://example.com/tudji-klub.jpg"], new DateTime(2026, 10, 3, 0, 0, 0, DateTimeKind.Utc));

    public static readonly Club ClubWithSecondExplorersJoinRequest;
    public static readonly Club ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest;
    public static readonly Club ClubWithTwoModeratorsAndDemoExplorerAsPlainMember;

    static ClubSeed()
    {
        ClubWithSecondExplorersJoinRequest = new(WellKnownUsers.Explorer, "Ljubitelji tvrđava", "Obilasci tvrđava uz Dunav.", ["https://example.com/tvrdjava.jpg"], new DateTime(2026, 10, 4, 0, 0, 0, DateTimeKind.Utc));
        ClubWithSecondExplorersJoinRequest.RequestToJoin(WellKnownUsers.SecondExplorer);

        ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest = new(WellKnownUsers.Explorer, "Biciklisti Dunavskog keja", "Vožnje biciklom uz Dunav.", ["https://example.com/kej.jpg"], new DateTime(2026, 10, 5, 0, 0, 0, DateTimeKind.Utc));
        ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest.RequestToJoin(WellKnownUsers.SecondExplorer);
        ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest.RequestToJoin(BlogSeed.IdentityDemoExplorer);

        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember = new(WellKnownUsers.Explorer, "Šetači Petrovaradina", "Šetnje podzemnim galerijama tvrđave.", ["https://example.com/petrovaradin.jpg"], new DateTime(2026, 10, 6, 0, 0, 0, DateTimeKind.Utc));
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.RequestToJoin(WellKnownUsers.SecondExplorer);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.RequestToJoin(WellKnownUsers.Administrator);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.AcceptJoinRequest(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.PromoteToModerator(WellKnownUsers.Explorer, WellKnownUsers.Administrator);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.RequestToJoin(BlogSeed.IdentityDemoExplorer);
        ClubWithTwoModeratorsAndDemoExplorerAsPlainMember.AcceptJoinRequest(WellKnownUsers.Explorer, BlogSeed.IdentityDemoExplorer);
    }

    public static Club[] All => [HikingClub, PhotographyClub, SecondExplorersClub, ClubWithSecondExplorersJoinRequest, ClubWithSecondExplorerAsMemberAndDemoExplorersJoinRequest, ClubWithTwoModeratorsAndDemoExplorerAsPlainMember];
}
