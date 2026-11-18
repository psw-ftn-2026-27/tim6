using Payment.Domain.Products;

namespace Payment.Application.Products;

public interface IProductRepository
{
    Task<Product?> GetByEntityIdAsync(Guid entityId);
    Task<List<Product>> GetByEntityIdsAsync(List<Guid> entityIds);
    void Add(Product product);
}
