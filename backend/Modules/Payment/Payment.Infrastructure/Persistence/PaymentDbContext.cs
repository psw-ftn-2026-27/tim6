using Microsoft.EntityFrameworkCore;
using Payment.Application;
using Payment.Domain.Carts;
using Payment.Domain.Products;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence.Carts;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentDbContext : DbContext, IUnitOfWork
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options) { }

    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("payment");
        builder.ConfigureWallets();
        builder.ConfigureProducts();
        builder.ConfigureCarts();
    }
}
