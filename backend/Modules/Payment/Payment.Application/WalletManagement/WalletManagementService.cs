using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletManagement;

public sealed class WalletManagementService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WalletManagementService(IWalletRepository walletRepository, IUnitOfWork unitOfWork)
    {
        _walletRepository = walletRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task DepositAsync(Guid walletId, Guid administratorId, DepositDto dto)
    {
        var wallet = await GetWalletAsync(walletId);

        wallet.Deposit(administratorId, dto.Amount, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CancelTransactionAsync(Guid walletId, Guid transactionId, CancellationDto dto)
    {
        var wallet = await GetWalletAsync(walletId);

        wallet.CancelTransaction(transactionId, dto.Reason, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Wallet> GetWalletAsync(Guid walletId)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet is null)
        {
            throw new NotFoundException($"Wallet {walletId} does not exist.");
        }

        return wallet;
    }
}
