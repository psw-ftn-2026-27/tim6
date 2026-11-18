using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.GiftCards;
using Payment.Infrastructure.Persistence.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureGiftCards();
        builder.ConfigureProducts();
    }
}
