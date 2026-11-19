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

    public async Task<IReadOnlyCollection<Product>> GetOwnedAsync(Guid ownerId)
    {
        return await _dbContext.Products
            .Where(product => product.OwnerId == ownerId)
            .ToListAsync();
    }

    public Task<Product?> GetByEntityIdAsync(Guid entityId)
    {
        return _dbContext.Products.FirstOrDefaultAsync(product => product.EntityId == entityId);
    }

    public Task<List<Product>> GetByEntityIdsAsync(List<Guid> entityIds)
    {
        return _dbContext.Products
            .Where(product => entityIds.Contains(product.EntityId))
            .ToListAsync();
    }

    public void Add(Product product)
    {
        _dbContext.Products.Add(product);
    }
}
