using Payment.Domain.Carts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Wallet : AggregateRoot
{
    public Guid UserId { get; private set; }
    private readonly List<Transaction> _transactions = [];
    public IReadOnlyList<Transaction> Transactions => _transactions;
    private readonly List<Payment> _payments = [];
    public IReadOnlyList<Payment> Payments => _payments;

    private Wallet() { }

    public Wallet(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public int Balance()
    {
        return _transactions
            .Where(transaction => !transaction.IsCancelled)
            .Sum(transaction => transaction.Amount)
            - _payments.Sum(payment => payment.Price);
    }

    public void Deposit(Guid administratorId, int amount, DateTime createdAt)
    {
        _transactions.Add(new Transaction(administratorId, amount, createdAt));
    }

    public void Spend(int amount, DateTime createdAt)
    {
        if (amount > Balance())
        {
            throw new DomainException("The wallet does not have enough EC.");
        }

        _transactions.Add(new Transaction(amount, createdAt));
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

    public void Pay(IReadOnlyList<CartItem> items, IReadOnlyDictionary<Guid, Guid> ownerIds, DateTime paidAt)
    {
        Pay(
            items.Select(item => new CartItemCalculation(
                item.EntityId,
                item.Name,
                item.Price,
                0,
                item.Price,
                item.Price,
                null)).ToList(),
            ownerIds,
            paidAt);
    }

    public void Pay(IReadOnlyList<CartItemCalculation> items, IReadOnlyDictionary<Guid, Guid> ownerIds, DateTime paidAt)
    {
        if (Balance() < items.Sum(item => item.WalletCharge))
        {
            throw new DomainException("The wallet balance is not enough to pay for the items.");
        }

        foreach (var item in items)
        {
            _payments.Add(new Payment(item.EntityId, item.Name, ownerIds[item.EntityId], item.WalletCharge, paidAt));
        }
    }
}
