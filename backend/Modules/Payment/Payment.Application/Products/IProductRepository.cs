using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByEntityIdAsync(Guid entityId);
    void Add(Product product);
}
