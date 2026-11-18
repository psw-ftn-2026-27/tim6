using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

public sealed class Product : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public int Price { get; private set; }
    public bool IsInOffer { get; private set; }

    private Product() { }

    public Product(Guid entityId, string name, Guid ownerId, int price) : base(Guid.NewGuid())
    {
        if (price < 0)
        {
            throw new DomainException("A product price cannot be less than 0.");
        }

        EntityId = entityId;
        Name = name;
        OwnerId = ownerId;
        Price = price;
        IsInOffer = true;
    }

    public void WithdrawFromOffer()
    {
        IsInOffer = false;
    }

    public void ReturnToOffer()
    {
        IsInOffer = true;
    }
}
