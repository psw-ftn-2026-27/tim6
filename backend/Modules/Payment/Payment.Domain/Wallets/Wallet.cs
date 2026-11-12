using Shared.Domain;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    public Guid UserId { get; private set; }
    private readonly List<Transaction> _transactions = [];
    public IReadOnlyList<Transaction> Transactions => _transactions;

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public int Balance()
    {
        return _transactions
            .Where(transaction => !transaction.IsCancelled)
            .Sum(transaction => transaction.Amount);
    }
}
