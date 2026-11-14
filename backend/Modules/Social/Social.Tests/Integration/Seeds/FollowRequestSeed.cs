using Shared.Tests;
using Social.Domain.Following;

namespace Social.Tests.Integration.Seeds;

/// <summary>
/// Follow requests of the baseline described in <see cref="FollowSeed"/>.
/// </summary>
internal static class FollowRequestSeed
{
    public static readonly FollowRequest SecondExplorerPendingRequestToExplorer =
        new(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);

    public static readonly FollowRequest ThirdExplorerPendingRequestToExplorer =
        new(FollowSeed.ThirdExplorer, WellKnownUsers.Explorer);

    public static readonly FollowRequest ExplorerPendingRequestToThirdExplorer =
        new(WellKnownUsers.Explorer, FollowSeed.ThirdExplorer);

    public static readonly FollowRequest ExplorerRejectedRequestToSecondExplorer;
    public static readonly FollowRequest AdministratorRejectedRequestToSecondExplorer;
    public static readonly FollowRequest AdministratorAcceptedRequestToSecondExplorer;
    public static readonly FollowRequest SecondExplorerAcceptedRequestToAdministrator;
    public static readonly FollowRequest AdministratorAcceptedRequestToThirdExplorer;
    public static readonly FollowRequest ThirdExplorerAcceptedRequestToSecondExplorer;

    static FollowRequestSeed()
    {
        ExplorerRejectedRequestToSecondExplorer = new(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);
        ExplorerRejectedRequestToSecondExplorer.Reject(WellKnownUsers.SecondExplorer);

        AdministratorRejectedRequestToSecondExplorer = new(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);
        AdministratorRejectedRequestToSecondExplorer.Reject(WellKnownUsers.SecondExplorer);

        AdministratorAcceptedRequestToSecondExplorer = new(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);
        AdministratorAcceptedRequestToSecondExplorer.Accept(WellKnownUsers.SecondExplorer);

        SecondExplorerAcceptedRequestToAdministrator = new FollowRequestProcessor()
            .Send(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator, null, null, FollowSeed.AdministratorFollowsSecondExplorer)
            .Request;

        AdministratorAcceptedRequestToThirdExplorer = new(WellKnownUsers.Administrator, FollowSeed.ThirdExplorer);
        AdministratorAcceptedRequestToThirdExplorer.Accept(FollowSeed.ThirdExplorer);

        ThirdExplorerAcceptedRequestToSecondExplorer = new(FollowSeed.ThirdExplorer, WellKnownUsers.SecondExplorer);
        ThirdExplorerAcceptedRequestToSecondExplorer.Accept(WellKnownUsers.SecondExplorer);
    }

    public static FollowRequest[] All =>
    [
        SecondExplorerPendingRequestToExplorer,
        ThirdExplorerPendingRequestToExplorer,
        ExplorerPendingRequestToThirdExplorer,
        ExplorerRejectedRequestToSecondExplorer,
        AdministratorRejectedRequestToSecondExplorer,
        AdministratorAcceptedRequestToSecondExplorer,
        SecondExplorerAcceptedRequestToAdministrator,
        AdministratorAcceptedRequestToThirdExplorer,
        ThirdExplorerAcceptedRequestToSecondExplorer
    ];
}
