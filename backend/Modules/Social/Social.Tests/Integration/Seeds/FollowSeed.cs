using Shared.Tests;
using Social.Domain.Following;

namespace Social.Tests.Integration.Seeds;

/// <summary>
/// Follow baseline built from the three well-known users plus <see cref="ThirdExplorer"/>, a user local to
/// this test project, because three users give only three pairs and the follow tests need six states.
/// Together with <see cref="FollowRequestSeed"/> it describes:
/// <list type="bullet">
/// <item>Administrator and Explorer have no relationship at all.</item>
/// <item>Administrator and SecondExplorer follow each other. Administrator's first request was rejected
/// and the second accepted; SecondExplorer's request was accepted at once.</item>
/// <item>SecondExplorer has a request to Explorer awaiting an answer, and an older request
/// from Explorer to SecondExplorer was rejected.</item>
/// <item>Administrator follows ThirdExplorer, who does not follow back.</item>
/// <item>ThirdExplorer follows SecondExplorer, who does not follow back.</item>
/// <item>ThirdExplorer and Explorer have requests to each other awaiting an answer, and neither follows the other.</item>
/// </list>
/// </summary>
internal static class FollowSeed
{
    /// <summary>
    /// A fourth user only this module's tests know. <see cref="WellKnownUsers"/> is platform-owned and has
    /// three users; feature modules store users as plain IDs, so this one needs no identity record.
    /// </summary>
    public static readonly Guid ThirdExplorer = Guid.Parse("00000000-0000-0000-0000-000000000004");

    public static readonly Follow AdministratorFollowsSecondExplorer =
        new(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);

    public static readonly Follow SecondExplorerFollowsAdministrator =
        new(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

    public static readonly Follow AdministratorFollowsThirdExplorer =
        new(WellKnownUsers.Administrator, ThirdExplorer);

    public static readonly Follow ThirdExplorerFollowsSecondExplorer =
        new(ThirdExplorer, WellKnownUsers.SecondExplorer);

    public static Follow[] All =>
    [
        AdministratorFollowsSecondExplorer,
        SecondExplorerFollowsAdministrator,
        AdministratorFollowsThirdExplorer,
        ThirdExplorerFollowsSecondExplorer
    ];
}
