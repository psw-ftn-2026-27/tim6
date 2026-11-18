namespace Payment.Contracts;

public interface IPaymentApi
{
    Task PutProductInOfferAsync(Guid entityId, string name, Guid ownerId, int price);
    Task WithdrawProductFromOfferAsync(Guid entityId);
    Task ReturnProductToOfferAsync(Guid entityId);
    Task<int> GetProductPriceAsync(Guid entityId);
}
