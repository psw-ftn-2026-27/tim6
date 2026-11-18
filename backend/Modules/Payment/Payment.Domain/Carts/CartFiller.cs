using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed class CartFiller
{
    public void AddProduct(Cart cart, Product product)
    {
        if (!product.IsInOffer)
        {
            throw new DomainException("A product that is not in the offer cannot be added to the cart.");
        }

        cart.AddItem(product.EntityId, product.Name, product.Price);
    }
}
