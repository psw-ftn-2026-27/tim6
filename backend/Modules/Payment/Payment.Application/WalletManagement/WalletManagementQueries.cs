using Identity.Contracts;
using Payment.Application.Wallets;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.WalletManagement;

public sealed class WalletManagementQueries
{
    private readonly IWalletReadRepository _walletReadRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IIdentityApi _identityApi;

    public WalletManagementQueries(
        IWalletReadRepository walletReadRepository,
        IWalletRepository walletRepository,
        IIdentityApi identityApi)
    {
        _walletReadRepository = walletReadRepository;
        _walletRepository = walletRepository;
        _identityApi = identityApi;
    }

    public async Task<PageResult<WalletSummaryDto>> GetAllAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var wallets = await _walletReadRepository.GetPageAsync(page, pageSize);
        var users = await _identityApi.GetUsersAsync(wallets.Items.Select(wallet => wallet.UserId));
        var emails = users.ToDictionary(user => user.UserId, user => user.Email);

        var items = wallets.Items
            .Select(wallet => new WalletSummaryDto(wallet.Id, emails[wallet.UserId], wallet.Balance))
            .ToList();
        return new PageResult<WalletSummaryDto>(items, wallets.TotalCount);
    }

    public async Task<WalletDetailsDto> GetByIdAsync(Guid id)
    {
        var wallet = await _walletRepository.GetByIdAsync(id);
        if (wallet is null)
        {
            throw new NotFoundException($"Wallet {id} does not exist.");
        }

        var transactions = await _walletReadRepository.GetTransactionsAsync(wallet.Id);
        var user = await _identityApi.GetUserAsync(wallet.UserId);
        return new WalletDetailsDto(wallet.Id, user.Email, wallet.Balance(), transactions);
    }
}
