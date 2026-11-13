using Shared.Tests;
using Social.Domain.Following;

namespace Social.Tests.Integration.Seeds;

/// <summary>
/// Follow baseline built from three users, so every state is reachable without a fourth well-known user:
/// <list type="bullet">
/// <item>Administrator and Explorer have no relationship at all.</item>
/// <item>Administrator and SecondExplorer follow each other. Administrator's first request was rejected
/// and the second accepted; SecondExplorer's request was accepted at once.</item>
/// <item>SecondExplorer has a request to Explorer awaiting an answer, and an older request
/// from Explorer to SecondExplorer was rejected.</item>
/// </list>
/// </summary>
internal static class FollowSeed
{
    public static readonly FollowRequest SecondExplorerPendingRequestToExplorer =
        new(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);

    public static readonly FollowRequest ExplorerRejectedRequestToSecondExplorer =
        CreateRejected(WellKnownUsers.Explorer, WellKnownUsers.SecondExplorer);

    public static readonly FollowRequest AdministratorRejectedRequestToSecondExplorer =
        CreateRejected(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);

    public static readonly FollowRequest AdministratorAcceptedRequestToSecondExplorer =
        CreateAccepted(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);

    public static readonly FollowRequest SecondExplorerAcceptedRequestToAdministrator =
        CreateAcceptedAutomatically(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

    public static readonly Follow AdministratorFollowsSecondExplorer =
        new(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);

    public static readonly Follow SecondExplorerFollowsAdministrator =
        new(WellKnownUsers.SecondExplorer, WellKnownUsers.Administrator);

    public static object[] All =>
    [
        SecondExplorerPendingRequestToExplorer,
        ExplorerRejectedRequestToSecondExplorer,
        AdministratorRejectedRequestToSecondExplorer,
        AdministratorAcceptedRequestToSecondExplorer,
        SecondExplorerAcceptedRequestToAdministrator,
        AdministratorFollowsSecondExplorer,
        SecondExplorerFollowsAdministrator
    ];

    private static FollowRequest CreateRejected(Guid senderId, Guid recipientId)
    {
        var request = new FollowRequest(senderId, recipientId);
        request.Reject(recipientId);
        return request;
    }

    private static FollowRequest CreateAccepted(Guid senderId, Guid recipientId)
    {
        var request = new FollowRequest(senderId, recipientId);
        request.Accept(recipientId);
        return request;
    }

    private static FollowRequest CreateAcceptedAutomatically(Guid senderId, Guid recipientId)
    {
        var request = new FollowRequest(senderId, recipientId);
        request.AcceptAutomatically();
        return request;
    }
}
