using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;

namespace Payment.Infrastructure.Persistence;

internal sealed class PaymentModuleInitializer : IHostedService
{
    private static readonly Guid DemoAdministrator = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid FirstDemoExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private static readonly Guid SecondDemoExplorer = Guid.Parse("20000000-0000-0000-0000-000000000002");
    private static readonly Guid[] DemoExplorers =
    [
        FirstDemoExplorer,
        SecondDemoExplorer,
        Guid.Parse("20000000-0000-0000-0000-000000000003")
    ];

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public PaymentModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
        await dbContext.Database.MigrateAsync();

        if (!_environment.IsDevelopment())
        {
            return;
        }

        var createdAt = DateTime.UtcNow;
        await CreateDemoWalletsAsync(dbContext, createdAt);
        await CreateDemoGiftCardAsync(dbContext, createdAt);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task CreateDemoWalletsAsync(PaymentDbContext dbContext, DateTime createdAt)
    {
        var existingUserIds = await dbContext.Wallets
            .Select(wallet => wallet.UserId)
            .ToListAsync();

        foreach (var explorerId in DemoExplorers.Where(explorerId => !existingUserIds.Contains(explorerId)))
        {
            var wallet = new Wallet(explorerId);
            wallet.Deposit(DemoAdministrator, 100, createdAt);
            dbContext.Wallets.Add(wallet);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task CreateDemoGiftCardAsync(PaymentDbContext dbContext, DateTime createdAt)
    {
        if (await DemoGiftCardExistsAsync(dbContext))
        {
            return;
        }

        var senderWallet = await dbContext.Wallets.SingleAsync(wallet => wallet.UserId == FirstDemoExplorer);
        senderWallet.Spend(50, createdAt);
        var giftCard = new GiftCard(
            senderWallet.UserId,
            SecondDemoExplorer,
            50,
            "Srećan put!",
            createdAt);

        dbContext.GiftCards.Add(giftCard);
        await dbContext.SaveChangesAsync();
    }

    private static Task<bool> DemoGiftCardExistsAsync(PaymentDbContext dbContext)
    {
        return dbContext.GiftCards.AnyAsync(giftCard =>
            giftCard.SenderId == FirstDemoExplorer &&
            giftCard.RecipientId == SecondDemoExplorer &&
            giftCard.Amount == 50);
    }
}
