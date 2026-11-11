using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.FollowRelationships;

namespace Social.Infrastructure.Persistence.FollowRelationships;

internal static class FollowRelationshipConfiguration
{
    public static void ConfigureFollowRelationships(this ModelBuilder modelBuilder)
    {
        ConfigureFollowRelationship(modelBuilder.Entity<FollowRelationship>());
        ConfigureFollowRequest(modelBuilder.Entity<FollowRequest>());
    }

    private static void ConfigureFollowRelationship(EntityTypeBuilder<FollowRelationship> builder)
    {
        builder.HasIndex(relationship => new { relationship.FirstUserId, relationship.SecondUserId }).IsUnique();

        builder.HasMany(relationship => relationship.Requests)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(relationship => relationship.Requests).AutoInclude();
    }

    private static void ConfigureFollowRequest(EntityTypeBuilder<FollowRequest> builder)
    {
        builder.ToTable("FollowRequests");
        builder.Property(request => request.Id).ValueGeneratedNever();
        builder.Property(request => request.Status).HasConversion<string>().HasMaxLength(20);
    }
}
