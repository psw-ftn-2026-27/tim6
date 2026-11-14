using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Following;

namespace Social.Infrastructure.Persistence.Following;

internal static class FollowConfiguration
{
    public static void ConfigureFollows(this ModelBuilder modelBuilder)
    {
        ConfigureFollow(modelBuilder.Entity<Follow>());
    }

    private static void ConfigureFollow(EntityTypeBuilder<Follow> builder)
    {
        builder.ToTable("Follows");
        builder.Property(follow => follow.Id).ValueGeneratedNever();

        builder.HasIndex(follow => new { follow.FollowerId, follow.FollowedUserId })
            .IsUnique()
            .HasDatabaseName("IX_Follows_FollowerId_FollowedUserId");
        builder.HasIndex(follow => follow.FollowedUserId);
    }
}
