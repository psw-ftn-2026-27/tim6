namespace Payment.Domain.Carts;

public sealed record CartItem
{
    public Guid EntityId { get; private init; }
    public string Name { get; private init; }
    public int Price { get; private init; }

    private CartItem()
    {
        Name = null!;
    }

    public CartItem(Guid entityId, string name, int price)
    {
        EntityId = entityId;
        Name = name;
        Price = price;
    }
}
