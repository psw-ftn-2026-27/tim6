using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application;
using Payment.Application.GiftCardBrowsing;
using Payment.Application.GiftCardSending;
using Payment.Application.GiftCards;
using Payment.Application.MyWallet;
using Payment.Application.WalletManagement;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Payment.Infrastructure.Persistence;
using Payment.Infrastructure.Persistence.GiftCards;
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
        services.AddScoped<IGiftCardRepository, GiftCardRepository>();
        services.AddScoped<IGiftCardReadRepository, GiftCardReadRepository>();

        services.AddScoped<MyWalletService>();
        services.AddScoped<MyWalletQueries>();
        services.AddScoped<WalletManagementService>();
        services.AddScoped<WalletManagementQueries>();
        services.AddScoped<GiftCardIssuer>();
        services.AddScoped<GiftCardSendingService>();
        services.AddScoped<GiftCardBrowsingQueries>();

        return services;
    }
}
