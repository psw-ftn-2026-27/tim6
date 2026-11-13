using Payment.Application.WalletManagement;
using Shared.Domain;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<List<TransactionDto>> GetTransactionsAsync(Guid walletId);
    Task<PageResult<WalletBalanceDto>> GetPageAsync(int page, int pageSize);
}
