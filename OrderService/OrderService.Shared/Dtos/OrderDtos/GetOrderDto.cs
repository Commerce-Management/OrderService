using OrderService.Shared.Dtos.OrderItemDtos;
using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.OrderDtos;

public record GetOrderDto(
    string Id,
    string UserId,
    string TrackingId,
    string Address,
    string City,
    int Zip,
    DateTime OrderDate,
    decimal TotalAmount,
    OrderStatus Status,
    ICollection<GetOrderItemDto> OrderItems
);