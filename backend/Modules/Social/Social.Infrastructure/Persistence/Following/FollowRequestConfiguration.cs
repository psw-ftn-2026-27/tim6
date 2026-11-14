using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Following;

namespace Social.Infrastructure.Persistence.Following;

internal static class FollowRequestConfiguration
{
    public static void ConfigureFollowRequests(this ModelBuilder modelBuilder)
    {
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
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
