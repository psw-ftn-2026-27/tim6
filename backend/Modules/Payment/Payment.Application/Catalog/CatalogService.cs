using Payment.Application.Products;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Application.Catalog;

public sealed class CatalogService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CatalogService(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task PutInOfferAsync(Guid entityId, string name, Guid ownerId, int price)
    {
        _productRepository.Add(new Product(entityId, name, ownerId, price));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task WithdrawFromOfferAsync(Guid entityId)
    {
        var product = await GetProductAsync(entityId);

        product.WithdrawFromOffer();
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReturnToOfferAsync(Guid entityId)
    {
        var product = await GetProductAsync(entityId);

        product.ReturnToOffer();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Product> GetProductAsync(Guid entityId)
    {
        var product = await _productRepository.GetByEntityIdAsync(entityId);
        if (product is null)
        {
            throw new NotFoundException($"Product for entity {entityId} does not exist.");
        }

        return product;
    }
}
