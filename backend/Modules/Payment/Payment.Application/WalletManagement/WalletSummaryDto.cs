namespace Payment.Application.WalletManagement;

public sealed record WalletSummaryDto(Guid Id, string? Email, int Balance);
