using Shared.Domain;
using Shared.Domain.Exceptions;

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

    public void Deposit(Guid administratorId, int amount, DateTime createdAt)
    {
        _transactions.Add(new Transaction(administratorId, amount, createdAt));
    }

    public void CancelTransaction(Guid transactionId, string reason, DateTime cancelledAt)
    {
        var transaction = _transactions.FirstOrDefault(transaction => transaction.Id == transactionId);
        if (transaction is null)
        {
            throw new NotFoundException($"Transaction {transactionId} does not exist in wallet {Id}.");
        }

        if (!transaction.IsCancelled && Balance() - transaction.Amount < 0)
        {
            throw new DomainException("A deposit cannot be cancelled when it would leave the wallet with a negative balance.");
        }

        transaction.Cancel(reason, cancelledAt);
    }
}
