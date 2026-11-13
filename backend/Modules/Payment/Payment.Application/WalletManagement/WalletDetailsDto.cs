using Payment.Application.Wallets;

namespace Payment.Application.WalletManagement;

public sealed record WalletDetailsDto(Guid Id, string? Email, int Balance, List<TransactionDto> Transactions);
