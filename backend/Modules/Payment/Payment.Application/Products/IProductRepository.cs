using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<IReadOnlyCollection<Product>> GetOwnedAsync(Guid ownerId);
    Task<Product?> GetByEntityIdAsync(Guid entityId);
    void Add(Product product);
}
