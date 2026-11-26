using Identity.Contracts;
using Payment.Application.GiftCards;
using Payment.Application.Wallets;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;

namespace Payment.Application.GiftCardSending;

public sealed class GiftCardSendingService
{
    private readonly IIdentityApi _identityApi;
    private readonly IWalletRepository _walletRepository;
    private readonly IGiftCardRepository _giftCardRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GiftCardSendingService(
        IIdentityApi identityApi,
        IWalletRepository walletRepository,
        IGiftCardRepository giftCardRepository,
        IUnitOfWork unitOfWork)
    {
        _identityApi = identityApi;
        _walletRepository = walletRepository;
        _giftCardRepository = giftCardRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateGiftCardAsync(Guid senderId, CreateGiftCardDto dto)
    {
        var recipient = await _identityApi.GetUserByEmailAsync(dto.RecipientEmail)
            ?? throw new NotFoundException("No user exists with that email.");
        var senderWallet = await _walletRepository.GetByUserIdAsync(senderId)
            ?? throw new NotFoundException("The sender does not have an open wallet.");

        senderWallet.Spend(dto.Amount, DateTime.UtcNow);
        var giftCard = new GiftCard(senderWallet.UserId, recipient.UserId, dto.Amount, dto.Message, DateTime.UtcNow);
        _giftCardRepository.Add(giftCard);

        await _unitOfWork.SaveChangesAsync();

        return giftCard.Id;
    }
}
