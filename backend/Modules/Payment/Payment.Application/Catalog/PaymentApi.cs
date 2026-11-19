using Payment.Application.Purchase;
using Payment.Contracts;

namespace Payment.Application.Catalog;

public sealed class PaymentApi : IPaymentApi
{
    private readonly CatalogService _catalogService;
    private readonly CatalogQueries _catalogQueries;
    private readonly PurchaseQueries _purchaseQueries;

    public PaymentApi(CatalogService catalogService, CatalogQueries catalogQueries, PurchaseQueries purchaseQueries)
    {
        _catalogService = catalogService;
        _catalogQueries = catalogQueries;
        _purchaseQueries = purchaseQueries;
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

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId)
    {
        return _purchaseQueries.HasPurchasedAsync(userId, entityId);
    }
}
