namespace Payment.Application.Products;

public sealed record ProductDto(Guid Id, Guid EntityId, string Name, int Price);
