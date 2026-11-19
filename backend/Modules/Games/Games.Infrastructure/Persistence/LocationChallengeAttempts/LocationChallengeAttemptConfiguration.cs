using Games.Domain.LocationChallengeAttempts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallengeAttempts;

internal static class LocationChallengeAttemptConfiguration
{
    public static void ConfigureLocationChallengeAttempts(this ModelBuilder modelBuilder)
    {
        ConfigureLocationChallengeAttempt(modelBuilder.Entity<LocationChallengeAttempt>());
    }

    private static void ConfigureLocationChallengeAttempt(EntityTypeBuilder<LocationChallengeAttempt> builder)
    {
        builder.ToTable("LocationChallengeAttempts");
        builder.HasIndex(attempt => new { attempt.LocationChallengeId, attempt.ExplorerId }).IsUnique();
        builder.PrimitiveCollection(attempt => attempt.RevealedHiddenPointIds)
            .HasField("_revealedHiddenPointIds");
    }
}
