using Microsoft.EntityFrameworkCore;
using Payment.Application.Products;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Products;

internal sealed class ProductReadRepository : IProductReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public ProductReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ProductDto>> GetOwnedAsync(Guid ownerId)
    {
        return _dbContext.Products
            .Where(product => product.OwnerId == ownerId)
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id)
            .Select(product => new ProductDto(product.Id, product.EntityId, product.Name, product.Price))
            .ToListAsync();
    }

    public async Task<PageResult<ProductDto>> GetInOfferPageAsync(int page, int pageSize)
    {
        var inOffer = _dbContext.Products
            .Where(product => product.IsInOffer)
            .OrderBy(product => product.Name)
            .ThenBy(product => product.Id);

        var totalCount = await inOffer.CountAsync();
        var items = await inOffer
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(product => new ProductDto(product.Id, product.EntityId, product.Name, product.Price))
            .ToListAsync();

        return new PageResult<ProductDto>(items, totalCount);
    }

    public Task<int?> GetPriceByEntityIdAsync(Guid entityId)
    {
        return _dbContext.Products
            .Where(product => product.EntityId == entityId)
            .Select(product => (int?)product.Price)
            .FirstOrDefaultAsync();
    }
}
