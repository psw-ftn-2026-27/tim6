using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.MyWallet;
using Shared.Api;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallets")]
[Authorize(Roles = "explorer")]
public sealed class MyWalletController : ControllerBase
{
    private readonly MyWalletService _myWalletService;
    private readonly MyWalletQueries _myWalletQueries;

    public MyWalletController(MyWalletService myWalletService, MyWalletQueries myWalletQueries)
    {
        _myWalletService = myWalletService;
        _myWalletQueries = myWalletQueries;
    }

    [HttpPost("mine")]
    public async Task<ActionResult> Open()
    {
        await _myWalletService.OpenAsync(User.GetUserId());
        return NoContent();
    }

    [HttpGet("mine")]
    public async Task<ActionResult<WalletDto>> Get()
    {
        return await _myWalletQueries.GetAsync(User.GetUserId());
    }
}
