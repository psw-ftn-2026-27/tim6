using Payment.Domain.Wallets;

namespace Payment.Domain.GiftCards;

public sealed class GiftCardIssuer
{
    public GiftCard Issue(
        Wallet senderWallet,
        Wallet recipientWallet,
        int amount,
        string message,
        DateTime createdAt)
    {
        var giftCard = new GiftCard(senderWallet.UserId, recipientWallet.UserId, amount, message, createdAt);
        senderWallet.Spend(amount, createdAt);
        return giftCard;
    }
}
