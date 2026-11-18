using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Positions;

namespace Shared.Infrastructure.Persistence.Positions;

internal static class ExplorerPositionConfiguration
{
    public static void ConfigureExplorerPositions(this ModelBuilder modelBuilder)
    {
        ConfigureExplorerPosition(modelBuilder.Entity<ExplorerPosition>());
    }

    private static void ConfigureExplorerPosition(EntityTypeBuilder<ExplorerPosition> builder)
    {
        builder.HasIndex(position => position.ExplorerId).IsUnique();

        builder.OwnsOne(position => position.Coordinates);
    }
}
