namespace OrderService.Shared.Dtos.OrderItemDtos;


public record CreateOrderItemDto(
    string ProductId,
    int Quantity
);