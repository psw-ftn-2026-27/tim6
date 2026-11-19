namespace Payment.Application.GiftCardSending;

public sealed record CreateGiftCardDto(string RecipientEmail, int Amount, string Message);
