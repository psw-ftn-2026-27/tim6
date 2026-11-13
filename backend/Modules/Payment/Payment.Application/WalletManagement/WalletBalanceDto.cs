namespace Payment.Application.WalletManagement;

public sealed record WalletBalanceDto(Guid Id, Guid UserId, int Balance);
