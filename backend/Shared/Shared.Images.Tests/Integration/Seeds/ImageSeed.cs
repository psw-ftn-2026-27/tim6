using Shared.Domain.Images;
using Shared.Tests;

namespace Shared.Images.Tests.Integration.Seeds;

internal static class ImageSeed
{
    public static readonly Image ExplorersFortressPhoto = new(WellKnownUsers.Explorer, ImageFormat.Png, new Dimensions(64, 48), 1024, 0);
    public static readonly Image SecondExplorersRiverbankPhoto = new(WellKnownUsers.SecondExplorer, ImageFormat.Jpeg, new Dimensions(32, 32), 512, 0);

    public static Image[] All => [ExplorersFortressPhoto, SecondExplorersRiverbankPhoto];
}
