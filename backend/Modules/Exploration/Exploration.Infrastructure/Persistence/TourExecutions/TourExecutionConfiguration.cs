using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal static class TourExecutionConfiguration
{
    public static void ConfigureTourExecutions(this ModelBuilder modelBuilder)
    {
        ConfigureTourExecution(modelBuilder.Entity<TourExecution>());
    }

    private static void ConfigureTourExecution(EntityTypeBuilder<TourExecution> builder)
    {
        builder.Property(execution => execution.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(execution => execution.TouristId)
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");

        builder.OwnsOne(execution => execution.StartPosition);

        builder.OwnsMany(execution => execution.CompletedKeyPoints, completedKeyPoints => completedKeyPoints.ToJson());
    }
}
