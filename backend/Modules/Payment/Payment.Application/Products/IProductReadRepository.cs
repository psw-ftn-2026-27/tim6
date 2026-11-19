using Shared.Domain;

namespace Payment.Application.Products;

public interface IProductReadRepository
{
    Task<List<ProductDto>> GetOwnedAsync(Guid ownerId);
    Task<PageResult<ProductDto>> GetInOfferPageAsync(int page, int pageSize);
    Task<int?> GetPriceByEntityIdAsync(Guid entityId);
}
