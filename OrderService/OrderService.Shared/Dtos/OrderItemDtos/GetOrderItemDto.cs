namespace OrderService.Shared.Dtos.OrderItemDtos;

public record GetOrderItemDto(
    string Id,
    string OrderId,
    string ProductId,
    string ShopId,
    int Quantity);
