namespace Payment.Application.Purchase;

public sealed record PurchaseDto(Guid Id, Guid EntityId, string Name, DateTime PurchasedAt);
