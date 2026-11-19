using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.CouponAuthoring;
using Payment.Application.Products;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/coupons")]
[Authorize(Roles = "explorer")]
public sealed class CouponAuthoringController : ControllerBase
{
    private readonly CouponAuthoringService _couponAuthoringService;
    private readonly CouponAuthoringQueries _couponAuthoringQueries;

    public CouponAuthoringController(
        CouponAuthoringService couponAuthoringService,
        CouponAuthoringQueries couponAuthoringQueries)
    {
        _couponAuthoringService = couponAuthoringService;
        _couponAuthoringQueries = couponAuthoringQueries;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCouponDto dto)
    {
        return await _couponAuthoringService.CreateCouponAsync(User.GetUserId(), dto);
    }

    [HttpGet("products")]
    public async Task<ActionResult<List<ProductDto>>> GetMyProducts()
    {
        return await _couponAuthoringQueries.GetMyProductsAsync(User.GetUserId());
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<CouponDto>>> GetMyCoupons(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _couponAuthoringQueries.GetMyCouponsAsync(User.GetUserId(), page, pageSize);
    }
}
