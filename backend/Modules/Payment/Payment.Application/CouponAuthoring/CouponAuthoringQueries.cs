using Payment.Application.Coupons;
using Payment.Application.Products;
using Shared.Domain;

namespace Payment.Application.CouponAuthoring;

public sealed class CouponAuthoringQueries
{
    private readonly IProductReadRepository _productReadRepository;
    private readonly ICouponReadRepository _couponReadRepository;

    public CouponAuthoringQueries(
        IProductReadRepository productReadRepository,
        ICouponReadRepository couponReadRepository)
    {
        _productReadRepository = productReadRepository;
        _couponReadRepository = couponReadRepository;
    }

    public Task<List<ProductDto>> GetMyProductsAsync(Guid ownerId)
    {
        return _productReadRepository.GetOwnedAsync(ownerId);
    }

    public Task<PageResult<CouponDto>> GetMyCouponsAsync(Guid ownerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _couponReadRepository.GetOwnedPageAsync(ownerId, page, pageSize);
    }
}
