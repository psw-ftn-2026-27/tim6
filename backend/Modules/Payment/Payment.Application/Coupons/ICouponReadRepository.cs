using Payment.Application.CouponAuthoring;
using Shared.Domain;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<PageResult<CouponDto>> GetOwnedPageAsync(Guid ownerId, int page, int pageSize);
}
