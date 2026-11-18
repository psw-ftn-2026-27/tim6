using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Payment.Domain.Products;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductRepository : IProductRepository
{
    private readonly PaymentDbContext _dbContext;

    public ProductRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Product?> GetByEntityIdAsync(Guid entityId)
    {
        return _dbContext.Products.FirstOrDefaultAsync(product => product.EntityId == entityId);
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }
}
