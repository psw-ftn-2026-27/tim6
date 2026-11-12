using Payment.Application.MyWallet;

namespace Payment.Application.Wallets;

public interface IWalletReadRepository
{
    Task<List<TransactionDto>> GetTransactionsAsync(Guid walletId);
}
