using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Purchase;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/purchases")]
[Authorize(Roles = "explorer")]
public sealed class PurchaseController : ControllerBase
{
    private readonly PurchaseService _purchaseService;
    private readonly PurchaseQueries _purchaseQueries;

    public PurchaseController(PurchaseService purchaseService, PurchaseQueries purchaseQueries)
    {
        _purchaseService = purchaseService;
        _purchaseQueries = purchaseQueries;
    }

    [HttpPost("mine")]
    public async Task<ActionResult> PurchaseCart()
    {
        await _purchaseService.PurchaseCartAsync(User.GetUserId());
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<List<PurchaseDto>>> GetMine()
    {
        return await _purchaseQueries.GetMineAsync(User.GetUserId());
    }
}
