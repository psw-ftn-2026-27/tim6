using Microsoft.EntityFrameworkCore;
using Payment.Application.CouponAuthoring;
using Payment.Application.Coupons;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _dbContext;

    public CouponReadRepository(PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PageResult<CouponDto>> GetOwnedPageAsync(Guid ownerId, int page, int pageSize)
    {
        var owned = _dbContext.Coupons
            .Where(coupon => coupon.OwnerId == ownerId)
            .OrderBy(coupon => coupon.Id);

        var totalCount = await owned.CountAsync();
        var items = await owned
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(coupon => new CouponDto(
                coupon.Id,
                coupon.OwnerId,
                coupon.ProductId,
                coupon.ProductId == null
                    ? null
                    : _dbContext.Products
                        .Where(product => product.Id == coupon.ProductId)
                        .Select(product => product.Name)
                        .FirstOrDefault(),
                coupon.DiscountPercentage,
                coupon.ValidUntil,
                coupon.Code))
            .ToListAsync();

        return new PageResult<CouponDto>(items, totalCount);
    }
}
