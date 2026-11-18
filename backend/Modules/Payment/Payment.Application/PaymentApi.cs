using Payment.Contracts;

namespace Payment.Application;

public sealed class PaymentApi : IPaymentApi
{
    public Task<bool> HasPurchasedTourAsync(Guid userId, Guid tourId)
    {
        // Privremeno: kupovina ture (P21) još ne postoji. Pravo ponašanje se dodaje uz P21.
        return Task.FromResult(true);
    }
}
