using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.Carts;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.PurchaseTokens;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Carts;
using Payment.Infrastructure.Persistence.Coupons;
using Payment.Infrastructure.Persistence.GiftCards;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.PurchaseTokens;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<GiftCard> GiftCards => Set<GiftCard>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<PurchaseToken> PurchaseTokens => Set<PurchaseToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureGiftCards();
        builder.ConfigureProducts();
        builder.ConfigureCoupons();
        builder.ConfigureCarts();
        builder.ConfigurePurchaseTokens();
    }
}
