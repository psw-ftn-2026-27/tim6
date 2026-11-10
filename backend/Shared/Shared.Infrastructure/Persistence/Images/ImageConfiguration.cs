using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Domain.Images;

namespace Shared.Infrastructure.Persistence.Images;

internal static class ImageConfiguration
{
    public static void ConfigureImages(this ModelBuilder modelBuilder)
    {
        ConfigureImage(modelBuilder.Entity<Image>());
    }

    private static void ConfigureImage(EntityTypeBuilder<Image> builder)
    {
        builder.Property(image => image.Format).HasConversion<string>().HasMaxLength(10);
        builder.Property(image => image.FileName).HasMaxLength(50);
        builder.HasIndex(image => image.FileName).IsUnique();

        builder.OwnsOne(image => image.Dimensions);
    }
}
