using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public sealed record GiftCardDto(
    Guid Id,
    Guid SenderId,
    string? SenderEmail,
    Guid RecipientId,
    string? RecipientEmail,
    int Amount,
    string Message,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    GiftCardStatus Status);
