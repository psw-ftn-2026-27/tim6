using Payment.Application.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.MyWallet;

public sealed class MyWalletQueries
{
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IWalletRepository _walletRepository;

    public MyWalletQueries(IWalletReadRepository walletReadRepository, IWalletRepository walletRepository)
    {
        _walletReadRepository = walletReadRepository;
        _walletRepository = walletRepository;
    }

    public async Task<WalletDto> GetAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            throw new NotFoundException($"User {userId} has no wallet.");
        }

        var transactions = await _walletReadRepository.GetTransactionsAsync(wallet.Id);
        return new WalletDto(wallet.Balance(), transactions);
    }
}
