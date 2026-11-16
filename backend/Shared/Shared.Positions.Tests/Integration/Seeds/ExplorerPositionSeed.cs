using Shared.Domain.Positions;
using Shared.Tests;

namespace Shared.Positions.Tests.Integration.Seeds;

internal static class ExplorerPositionSeed
{
    public static readonly ExplorerPosition ExplorerAtTheFortress = new(WellKnownUsers.Explorer, new Coordinates(45.2517, 19.8661));

    public static ExplorerPosition[] All => [ExplorerAtTheFortress];
}
