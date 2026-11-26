namespace Payment.Domain.GiftCards;

public sealed record GiftCardSendingDetails(
    Guid RecipientId,
    int Amount,
    string Message,
    DateTime CreatedAt);
