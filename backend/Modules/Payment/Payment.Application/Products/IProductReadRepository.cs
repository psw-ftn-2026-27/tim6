using Payment.Application.Catalog;
using Shared.Domain;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<PageResult<ProductDto>> GetInOfferPageAsync(int page, int pageSize);
    Task<int?> GetPriceByEntityIdAsync(Guid entityId);
}
