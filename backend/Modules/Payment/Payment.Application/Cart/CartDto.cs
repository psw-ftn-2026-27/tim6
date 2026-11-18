namespace Payment.Application.Cart;

public sealed record CartDto(List<CartItemDto> Items, int TotalPrice);
