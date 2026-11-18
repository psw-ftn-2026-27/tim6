using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Carts;

public sealed class Cart : AggregateRoot
{
    public Guid UserId { get; private set; }
    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items;

    private Cart() { }

    public Cart(Guid userId) : base(Guid.NewGuid())
    {
        UserId = userId;
    }

    public int TotalPrice()
    {
        return _items.Sum(item => item.Price);
    }

    public void AddItem(Guid entityId, string name, int price)
    {
        if (_items.Any(item => item.EntityId == entityId))
        {
            throw new DomainException("The product is already in the cart.");
        }

        _items.Add(new CartItem(entityId, name, price));
    }

    public void RemoveItem(Guid entityId)
    {
        var item = _items.FirstOrDefault(item => item.EntityId == entityId);
        if (item is null)
        {
            throw new NotFoundException($"Entity {entityId} is not in cart {Id}.");
        }

        _items.Remove(item);
    }

    public void Checkout()
    {
        if (_items.Count == 0)
        {
            throw new DomainException("An empty cart cannot be purchased.");
        }

        _items.Clear();
    }
}
