using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCardIssuer
{
    public GiftCard Issue(
        Wallet senderWallet,
        Wallet? recipientWallet,
        int amount,
        string message,
        DateTime createdAt)
    {
        if (recipientWallet is null) throw new DomainException("Gift card recipient must have an open wallet.");

        var giftCard = new GiftCard(senderWallet.UserId, recipientWallet.UserId, amount, message, createdAt);
        senderWallet.Spend(amount, createdAt);

        return giftCard;
    }
}
