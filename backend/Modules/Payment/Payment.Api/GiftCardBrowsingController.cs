using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardBrowsing;
using Payment.Application.GiftCards;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardBrowsingController : ControllerBase
{
    private readonly GiftCardBrowsingQueries _giftCardBrowsingQueries;

    public GiftCardBrowsingController(GiftCardBrowsingQueries giftCardBrowsingQueries)
    {
        _giftCardBrowsingQueries = giftCardBrowsingQueries;
    }

    [HttpGet("sent")]
    public async Task<ActionResult<PageResult<GiftCardDto>>> GetSent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _giftCardBrowsingQueries.GetSentGiftCardsAsync(User.GetUserId(), page, pageSize);
    }

    [HttpGet("received")]
    public async Task<ActionResult<PageResult<GiftCardDto>>> GetReceived(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _giftCardBrowsingQueries.GetReceivedGiftCardsAsync(User.GetUserId(), page, pageSize);
    }
}
