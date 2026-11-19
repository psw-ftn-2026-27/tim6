using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Carts;

namespace Payment.Infrastructure.Persistence.Carts;

internal static class CartConfiguration
{
    public static void ConfigureCarts(this ModelBuilder modelBuilder)
    {
        ConfigureCart(modelBuilder.Entity<Cart>());
    }

    private static void ConfigureCart(EntityTypeBuilder<Cart> builder)
    {
        builder.HasIndex(cart => cart.UserId).IsUnique();

        builder.OwnsMany(cart => cart.Items, items =>
        {
            items.ToJson();
        });
    }
}
