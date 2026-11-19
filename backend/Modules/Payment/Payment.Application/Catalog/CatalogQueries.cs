using Payment.Application.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Application.Catalog;

public sealed class CatalogQueries
{
    private readonly IProductReadRepository _productReadRepository;

    public CatalogQueries(IProductReadRepository productReadRepository)
    {
        _productReadRepository = productReadRepository;
    }

    public Task<PageResult<ProductDto>> GetOfferAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _productReadRepository.GetInOfferPageAsync(page, pageSize);
    }

    public async Task<int> GetPriceAsync(Guid entityId)
    {
        var price = await _productReadRepository.GetPriceByEntityIdAsync(entityId);
        if (price is null)
        {
            throw new NotFoundException($"Product for entity {entityId} does not exist.");
        }

        return price.Value;
    }
}
