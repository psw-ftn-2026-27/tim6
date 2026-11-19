namespace Payment.Application.Carts;

public interface ICartRepository
{
    Task<Payment.Domain.Carts.Cart?> GetByUserIdAsync(Guid userId);
    void Add(Payment.Domain.Carts.Cart cart);
}
