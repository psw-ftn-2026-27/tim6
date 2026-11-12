namespace Payment.Application.MyWallet;

public sealed record WalletDto(int Balance, List<TransactionDto> Transactions);
