namespace Payment.Application.Wallets;

public sealed record TransactionDto(
    Guid Id,
    int Amount,
    DateTime CreatedAt,
    bool IsCancelled,
    DateTime? CancelledAt,
    string? CancellationReason);
