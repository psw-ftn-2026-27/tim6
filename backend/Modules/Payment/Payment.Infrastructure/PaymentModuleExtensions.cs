using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.Catalog;
using Payment.Application.CouponAuthoring;
using Payment.Application.Coupons;
using Payment.Application.MyWallet;
using Payment.Application.Products;
using Payment.Application.WalletManagement;
using Payment.Application.Wallets;
using Payment.Contracts;
using Payment.Domain.Coupons;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.Coupons;
using Payment.Infrastructure.Persistence.Products;
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
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICouponReadRepository, CouponReadRepository>();

        services.AddScoped<MyWalletService>();
        services.AddScoped<MyWalletQueries>();
        services.AddScoped<WalletManagementService>();
        services.AddScoped<WalletManagementQueries>();
        services.AddScoped<CatalogService>();
        services.AddScoped<CatalogQueries>();
        services.AddScoped<CouponIssuer>();
        services.AddScoped<CouponAuthoringService>();
        services.AddScoped<CouponAuthoringQueries>();

        services.AddScoped<IPaymentApi, PaymentApi>();

        return services;
    }
}
