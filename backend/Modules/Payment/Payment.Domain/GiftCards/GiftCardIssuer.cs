using Payment.Domain.Wallets;

namespace Payment.Domain.GiftCards;

public sealed class GiftCardIssuer
{
    public GiftCard Issue(Wallet senderWallet, GiftCardSendingDetails details)
    {
        senderWallet.Spend(details.Amount, details.CreatedAt);

        return new GiftCard(
            senderWallet.UserId,
            details.RecipientId,
            details.Amount,
            details.Message,
            details.CreatedAt);
    }
}
