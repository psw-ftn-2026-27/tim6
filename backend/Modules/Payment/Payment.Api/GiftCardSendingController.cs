using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.GiftCardSending;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class GiftCardSendingController : ControllerBase
{
    private readonly GiftCardSendingService _giftCardSendingService;

    public GiftCardSendingController(GiftCardSendingService giftCardSendingService)
    {
        _giftCardSendingService = giftCardSendingService;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateGiftCardDto dto)
    {
        return await _giftCardSendingService.CreateGiftCardAsync(User.GetUserId(), dto);
    }
}
