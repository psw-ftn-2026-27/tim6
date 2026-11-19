using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Catalog;
using Payment.Application.Products;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/products")]
[Authorize]
public sealed class CatalogController : ControllerBase
{
    private readonly CatalogQueries _catalogQueries;

    public CatalogController(CatalogQueries catalogQueries)
    {
        _catalogQueries = catalogQueries;
    }

    [HttpGet("offer")]
    public async Task<ActionResult<PageResult<ProductDto>>> GetOffer([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        return await _catalogQueries.GetOfferAsync(page, pageSize);
    }
}
