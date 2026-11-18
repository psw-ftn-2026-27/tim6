namespace Payment.Application.Catalog;

public sealed record ProductDto(Guid Id, Guid EntityId, string Name, int Price);
