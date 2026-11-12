using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal static class WalletConfiguration
{
    public static void ConfigureWallets(this ModelBuilder modelBuilder)
    {
        ConfigureWallet(modelBuilder.Entity<Wallet>());
        ConfigureTransaction(modelBuilder.Entity<Transaction>());
    }

    private static void ConfigureWallet(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasIndex(wallet => wallet.UserId).IsUnique();

        builder.HasMany(wallet => wallet.Transactions)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(wallet => wallet.Transactions).AutoInclude();
    }

    private static void ConfigureTransaction(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.Property(transaction => transaction.Id).ValueGeneratedNever();
    }
}
