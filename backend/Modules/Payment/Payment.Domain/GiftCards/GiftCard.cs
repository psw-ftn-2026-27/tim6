using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCard : AggregateRoot
{
    public Guid SenderId { get; private set; }
    public Guid RecipientId { get; private set; }
    public int Amount { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRedeemed { get; private set; }

    private GiftCard() { }

    public GiftCard(Guid senderId, Guid recipientId, int amount, string message, DateTime createdAt)
        : base(Guid.NewGuid())
    {
        if (senderId == recipientId)
        {
            throw new DomainException("You can't send a gift card yourself.");
        }

        if (amount is < 10 or > 1000)
        {
            throw new DomainException("A gift card amount must be between 10 and 1000 EC.");
        }

        if (string.IsNullOrWhiteSpace(message) || message.Length > 200)
        {
            throw new DomainException("A gift card message is required and cannot exceed 200 characters.");
        }

        SenderId = senderId;
        RecipientId = recipientId;
        Amount = amount;
        Message = message;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.AddYears(3);
    }

    public void Redeem(DateTime redeemedAt)
    {
        IsRedeemed = true;
    }

    public GiftCardStatus Status(DateTime now)
    {
        if (IsRedeemed)
        {
            return GiftCardStatus.Redeemed;
        }

        return now >= ExpiresAt ? GiftCardStatus.Expired : GiftCardStatus.Valid;
    }
}
