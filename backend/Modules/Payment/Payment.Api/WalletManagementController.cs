using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.WalletManagement;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/wallets")]
[Authorize(Roles = "administrator")]
public sealed class WalletManagementController : ControllerBase
{
    private readonly WalletManagementService _walletManagementService;
    private readonly WalletManagementQueries _walletManagementQueries;

    public WalletManagementController(
        WalletManagementService walletManagementService,
        WalletManagementQueries walletManagementQueries)
    {
        _walletManagementService = walletManagementService;
        _walletManagementQueries = walletManagementQueries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<WalletSummaryDto>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _walletManagementQueries.GetAllAsync(page, pageSize);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WalletDetailsDto>> GetById(Guid id)
    {
        return await _walletManagementQueries.GetByIdAsync(id);
    }

    [HttpPost("{id:guid}/deposits")]
    public async Task<ActionResult> Deposit(Guid id, DepositDto dto)
    {
        await _walletManagementService.DepositAsync(id, User.GetUserId(), dto);
        return NoContent();
    }

    [HttpPost("{id:guid}/transactions/{transactionId:guid}/cancel")]
    public async Task<ActionResult> CancelTransaction(Guid id, Guid transactionId, CancellationDto dto)
    {
        await _walletManagementService.CancelTransactionAsync(id, transactionId, dto);
        return NoContent();
    }
}
