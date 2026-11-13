using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Wallets;

public sealed class Transaction : Entity
{
    public Guid AdministratorId { get; private set; }
    public int Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsCancelled { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    private Transaction() { }

    internal Transaction(Guid administratorId, int amount, DateTime createdAt) : base(Guid.NewGuid())
    {
        if (amount < 1)
        {
            throw new DomainException("A deposit requires an amount of at least 1.");
        }

        AdministratorId = administratorId;
        Amount = amount;
        CreatedAt = createdAt;
    }

    internal void Cancel(string reason, DateTime cancelledAt)
    {
        if (IsCancelled)
        {
            throw new DomainException("A deposit that is already cancelled cannot be cancelled again.");
        }

        if (cancelledAt - CreatedAt > TimeSpan.FromHours(24))
        {
            throw new DomainException("A deposit can only be cancelled within 24 hours of being made.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException("A cancellation requires a reason.");
        }

        IsCancelled = true;
        CancelledAt = cancelledAt;
        CancellationReason = reason;
    }
}
