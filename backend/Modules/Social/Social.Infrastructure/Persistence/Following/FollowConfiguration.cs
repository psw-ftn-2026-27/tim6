using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Following;

namespace Social.Infrastructure.Persistence.Following;

internal static class FollowConfiguration
{
    public static void ConfigureFollowing(this ModelBuilder modelBuilder)
    {
        ConfigureFollow(modelBuilder.Entity<Follow>());
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
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

    private static void ConfigureFollowRequest(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequests");
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(request => request.SenderId);
        builder.HasIndex(request => request.RecipientId);
        builder.HasIndex(request => new { request.SenderId, request.RecipientId })
            .IsUnique()
            .HasDatabaseName("IX_FollowRequests_SenderId_RecipientId_Pending")
            .HasFilter($"\"Status\" = '{nameof(FollowRequestStatus.Pending)}'");
    }
}
