namespace Payment.Application.MyWallet;

public sealed record TransactionDto(
    Guid Id,
    int Amount,
    DateTime CreatedAt,
    bool IsCancelled,
    DateTime? CancelledAt,
    string? CancellationReason);
