using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.Tours;

internal static class TourConfiguration
{
    public static void ConfigureTours(this ModelBuilder modelBuilder)
    {
        ConfigureTour(modelBuilder.Entity<Tour>());
        ConfigureKeyPoint(modelBuilder.Entity<KeyPoint>());
    }

    private static void ConfigureTour(EntityTypeBuilder<Tour> builder)
    {
        builder.Property(tour => tour.Name).HasMaxLength(200);
        builder.Property(tour => tour.Difficulty).HasConversion<string>().HasMaxLength(20);
        builder.Property(tour => tour.Status).HasConversion<string>().HasMaxLength(20);

        builder.OwnsMany(tour => tour.TransportTimes, transportTimes =>
        {
            transportTimes.ToJson();
            transportTimes.Property(time => time.Transport).HasConversion<string>();
        });

        builder.OwnsOne(tour => tour.RecommendedGroupSize, groupSize =>
        {
            groupSize.Property(size => size.Min).HasDefaultValue(1);
            groupSize.Property(size => size.Max).HasDefaultValue(10);
        });

        builder.OwnsOne(tour => tour.Length);

        builder.HasMany(tour => tour.KeyPoints)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(tour => tour.KeyPoints).AutoInclude();
    }

    private static void ConfigureKeyPoint(EntityTypeBuilder<KeyPoint> builder)
    {
        builder.ToTable("KeyPoints");
        builder.Property(keyPoint => keyPoint.Id).ValueGeneratedNever();
        builder.Property(keyPoint => keyPoint.Name).HasMaxLength(200);

        builder.OwnsOne(keyPoint => keyPoint.Location);
    }
}
