using Payment.Application.Wallets;
using Payment.Domain.Wallets;

namespace Payment.Application.MyWallet;

public sealed class MyWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public MyWalletService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task OpenAsync(Guid userId)
    {
        var wallet = await _walletRepository.GetByUserIdAsync(userId);
        if (wallet is null)
        {
            _walletRepository.Add(new Wallet(userId));
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
