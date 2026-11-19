using Games.Domain.ChallengeProgresses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.ChallengeProgresses;

internal static class ChallengeProgressConfiguration
{
    public static void ConfigureChallengeProgresses(this ModelBuilder modelBuilder)
    {
        ConfigureChallengeProgress(modelBuilder.Entity<ChallengeProgress>());
    }

    private static void ConfigureChallengeProgress(EntityTypeBuilder<ChallengeProgress> builder)
    {
        builder.ToTable("ChallengeProgresses");
        builder.HasIndex(progress => new { progress.ChallengeId, progress.TouristId }).IsUnique();
    }
}
