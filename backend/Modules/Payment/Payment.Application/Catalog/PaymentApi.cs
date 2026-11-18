using Payment.Contracts;

namespace Payment.Application.Catalog;

public sealed class PaymentApi : IPaymentApi
{
    private readonly CatalogService _catalogService;
    private readonly CatalogQueries _catalogQueries;

    public PaymentApi(CatalogService catalogService, CatalogQueries catalogQueries)
    {
        _catalogService = catalogService;
        _catalogQueries = catalogQueries;
    }

    public Task PutProductInOfferAsync(Guid entityId, string name, Guid ownerId, int price)
    {
        return _catalogService.PutInOfferAsync(entityId, name, ownerId, price);
    }

    public Task WithdrawProductFromOfferAsync(Guid entityId)
    {
        return _catalogService.WithdrawFromOfferAsync(entityId);
    }

    public Task ReturnProductToOfferAsync(Guid entityId)
    {
        return _catalogService.ReturnToOfferAsync(entityId);
    }

    public Task<int> GetProductPriceAsync(Guid entityId)
    {
        return _catalogQueries.GetPriceAsync(entityId);
    }
}
