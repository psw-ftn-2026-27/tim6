namespace Payment.Contracts;

public interface IPaymentApi
{
    Task<bool> HasPurchasedTourAsync(Guid userId, Guid tourId);
}
