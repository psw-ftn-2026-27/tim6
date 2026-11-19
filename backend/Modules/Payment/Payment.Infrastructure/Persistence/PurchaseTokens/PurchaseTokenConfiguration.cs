using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.PurchaseTokens;

namespace Payment.Infrastructure.Persistence.PurchaseTokens;

internal static class PurchaseTokenConfiguration
{
    public static void ConfigurePurchaseTokens(this ModelBuilder modelBuilder)
    {
        ConfigurePurchaseToken(modelBuilder.Entity<PurchaseToken>());
    }

    private static void ConfigurePurchaseToken(EntityTypeBuilder<PurchaseToken> builder)
    {
        builder.HasIndex(token => new { token.UserId, token.EntityId });
    }
}
