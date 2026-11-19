using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Domain.Coupons;

namespace Payment.Application.CouponAuthoring;

public sealed class CouponAuthoringService
{
    private readonly IProductRepository _productRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly CouponIssuer _couponIssuer;
    private readonly IUnitOfWork _unitOfWork;

    public CouponAuthoringService(
        IProductRepository productRepository,
        ICouponRepository couponRepository,
        CouponIssuer couponIssuer,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _couponRepository = couponRepository;
        _couponIssuer = couponIssuer;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateCouponAsync(Guid ownerId, CreateCouponDto dto)
    {
        var ownedProducts = await _productRepository.GetOwnedAsync(ownerId);
        var details = new CouponIssueDetails(
            ownerId,
            dto.ProductId,
            dto.DiscountPercentage,
            dto.ValidUntil,
            DateTime.UtcNow);
        var coupon = _couponIssuer.Issue(ownedProducts, details);

        _couponRepository.Add(coupon);
        await _unitOfWork.SaveChangesAsync();

        return coupon.Id;
    }
}
