using Payment.Domain.Wallets;

namespace Payment.Application.Wallets;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserIdAsync(Guid userId);
    void Add(Wallet wallet);
}
