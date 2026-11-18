using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Social.Domain.Clubs;

namespace Social.Infrastructure.Persistence.Clubs;

internal static class ClubConfiguration
{
    public static void ConfigureClubs(this ModelBuilder modelBuilder)
    {
        ConfigureClub(modelBuilder.Entity<Club>());
        ConfigureClubMember(modelBuilder.Entity<ClubMember>());
    }

    private static void ConfigureClub(EntityTypeBuilder<Club> builder)
    {
        builder.ToTable("Clubs");
        builder.Property(club => club.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(club => club.Members)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(club => club.Members).AutoInclude();

        builder.OwnsMany(club => club.JoinRequests, joinRequests =>
        {
            joinRequests.ToJson();
        });
    }

    private static void ConfigureClubMember(EntityTypeBuilder<ClubMember> builder)
    {
        builder.ToTable("ClubMembers");
        builder.Property(member => member.Id).ValueGeneratedNever();
        builder.Property(member => member.Role).HasConversion<string>().HasMaxLength(20);
    }
}
