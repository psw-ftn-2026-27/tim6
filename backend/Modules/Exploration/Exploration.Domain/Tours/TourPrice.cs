using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourPrice
{
    private const int MinimumPaidAmount = 1;

    public bool IsFree { get; private init; }
    public int? Amount { get; private init; }

    public TourPrice(bool isFree, decimal? amount)
    {
        if (isFree && amount is not null)
        {
            throw new DomainException("A free tour cannot have an amount.");
        }
        if (!isFree && (amount is null || amount < MinimumPaidAmount || amount != decimal.Truncate(amount.Value)))
        {
            throw new DomainException($"A paid tour requires a whole amount of at least {MinimumPaidAmount} EC.");
        }

        IsFree = isFree;
        Amount = amount is null ? null : (int)amount.Value;
    }

    public int GetOfferPrice()
    {
        return Amount ?? 0;
    }
}
