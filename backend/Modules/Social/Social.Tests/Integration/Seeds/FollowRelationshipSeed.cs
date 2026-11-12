using Shared.Tests;
using Social.Domain.FollowRelationships;

namespace Social.Tests.Integration.Seeds;

internal static class FollowRelationshipSeed
{
    public static readonly FollowRelationship SecondExplorerRequestedExplorer;
    public static readonly FollowRelationship AdministratorAndSecondExplorerFollowEachOther;

    static FollowRelationshipSeed()
    {
        SecondExplorerRequestedExplorer = new(WellKnownUsers.SecondExplorer, WellKnownUsers.Explorer);
        SecondExplorerRequestedExplorer.SendRequest(WellKnownUsers.SecondExplorer);

        AdministratorAndSecondExplorerFollowEachOther = new(WellKnownUsers.Administrator, WellKnownUsers.SecondExplorer);
        var administratorRequest =
            AdministratorAndSecondExplorerFollowEachOther.SendRequest(WellKnownUsers.Administrator);
        AdministratorAndSecondExplorerFollowEachOther.AcceptRequest(administratorRequest.Id, WellKnownUsers.SecondExplorer);
        AdministratorAndSecondExplorerFollowEachOther.SendRequest(WellKnownUsers.SecondExplorer);
    }

    public static FollowRelationship[] All =>
        [SecondExplorerRequestedExplorer, AdministratorAndSecondExplorerFollowEachOther];
}
