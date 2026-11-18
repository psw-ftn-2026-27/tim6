using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.Cart;
using Payment.Application.Carts;
using Payment.Application.Catalog;
using Payment.Application.MyWallet;
using Payment.Application.Products;
using Payment.Application.Purchase;
using Payment.Application.PurchaseTokens;
using Payment.Application.WalletManagement;
using Payment.Application.Wallets;
using Payment.Contracts;
using Payment.Domain.Carts;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Carts;
using Payment.Infrastructure.Persistence.Products;
using Payment.Infrastructure.Persistence.PurchaseTokens;
using Payment.Infrastructure.Persistence.Wallets;
using Shared.Infrastructure;

namespace Payment.Infrastructure;

public static class PaymentModuleExtensions
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentDbContext>(configuration, "payment");
        services.AddHostedService<PaymentModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<PaymentMapperProfile>());

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PaymentDbContext>());
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletReadRepository, WalletReadRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductReadRepository, ProductReadRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<ICartReadRepository, CartReadRepository>();
        services.AddScoped<IPurchaseTokenRepository, PurchaseTokenRepository>();
        services.AddScoped<IPurchaseTokenReadRepository, PurchaseTokenReadRepository>();

        services.AddScoped<CartFiller>();
        services.AddScoped<CartCheckout>();

        services.AddScoped<MyWalletService>();
        services.AddScoped<MyWalletQueries>();
        services.AddScoped<WalletManagementService>();
        services.AddScoped<WalletManagementQueries>();
        services.AddScoped<CatalogService>();
        services.AddScoped<CatalogQueries>();
        services.AddScoped<CartService>();
        services.AddScoped<CartQueries>();
        services.AddScoped<PurchaseService>();
        services.AddScoped<PurchaseQueries>();

        services.AddScoped<IPaymentApi, PaymentApi>();

        return services;
    }
}
