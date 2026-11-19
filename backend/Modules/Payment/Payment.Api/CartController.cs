using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Cart;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/carts")]
[Authorize(Roles = "explorer")]
public sealed class CartController : ControllerBase
{
    private readonly CartService _cartService;
    private readonly CartQueries _cartQueries;

    public CartController(CartService cartService, CartQueries cartQueries)
    {
        _cartService = cartService;
        _cartQueries = cartQueries;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<CartDto>> Get(
        [FromQuery] string? couponCode = null,
        [FromQuery] Guid? giftCardId = null)
    {
        return await _cartQueries.GetAsync(User.GetUserId(), couponCode, giftCardId);
    }

    [HttpPost("mine/items/{entityId:guid}")]
    public async Task<ActionResult> AddProduct(Guid entityId)
    {
        await _cartService.AddProductAsync(User.GetUserId(), entityId);
        return NoContent();
    }

    [HttpDelete("mine/items/{entityId:guid}")]
    public async Task<ActionResult> RemoveItem(Guid entityId)
    {
        await _cartService.RemoveItemAsync(User.GetUserId(), entityId);
        return NoContent();
    }
}
