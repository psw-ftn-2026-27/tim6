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
        ConfigurePayment(modelBuilder.Entity<Payment.Domain.Wallets.Payment>());
    }

    private static void ConfigureWallet(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasIndex(wallet => wallet.UserId).IsUnique();

        builder.HasMany(wallet => wallet.Transactions)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(wallet => wallet.Transactions).AutoInclude();

        builder.HasMany(wallet => wallet.Payments)
            .WithOne()
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(wallet => wallet.Payments).AutoInclude();
    }

    private static void ConfigureTransaction(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.Property(transaction => transaction.Id).ValueGeneratedNever();
    }

    private static void ConfigurePayment(EntityTypeBuilder<Payment.Domain.Wallets.Payment> builder)
    {
        builder.ToTable("Payments");
        builder.Property(payment => payment.Id).ValueGeneratedNever();
    }
}
