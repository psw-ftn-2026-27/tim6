using Microsoft.EntityFrameworkCore;
using Payment.Application.MyWallet;
using Payment.Application.Wallets;

namespace Payment.Infrastructure.Persistence.Wallets;

internal sealed class WalletReadRepository : IWalletReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public WalletReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TransactionDto>> GetTransactionsAsync(Guid walletId)
    {
        return _dbContext.Wallets
            .Where(wallet => wallet.Id == walletId)
            .SelectMany(wallet => wallet.Transactions)
            .OrderByDescending(transaction => transaction.CreatedAt)
            .Select(transaction => new TransactionDto(
                transaction.Id,
                transaction.Amount,
                transaction.CreatedAt,
                transaction.IsCancelled,
                transaction.CancelledAt,
                transaction.CancellationReason))
            .ToListAsync();
    }
}
