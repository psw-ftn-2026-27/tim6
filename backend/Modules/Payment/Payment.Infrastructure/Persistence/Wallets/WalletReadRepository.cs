using Microsoft.EntityFrameworkCore;
using Payment.Application.WalletManagement;
using Payment.Application.Wallets;
using Shared.Domain;

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

    public async Task<PageResult<WalletBalanceDto>> GetPageAsync(int page, int pageSize)
    {
        var orderedWallets = _dbContext.Wallets.OrderBy(wallet => wallet.Id);

        var totalCount = await orderedWallets.CountAsync();
        var items = await orderedWallets
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(wallet => new WalletBalanceDto(
                wallet.Id,
                wallet.UserId,
                wallet.Transactions
                    .Where(transaction => !transaction.IsCancelled)
                    .Sum(transaction => transaction.Amount)))
            .ToListAsync();

        return new PageResult<WalletBalanceDto>(items, totalCount);
    }
}
