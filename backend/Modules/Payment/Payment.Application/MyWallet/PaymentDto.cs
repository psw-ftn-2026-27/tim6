namespace Payment.Application.MyWallet;

public sealed record PaymentDto(Guid Id, Guid EntityId, string Name, int Price, DateTime PaidAt);
