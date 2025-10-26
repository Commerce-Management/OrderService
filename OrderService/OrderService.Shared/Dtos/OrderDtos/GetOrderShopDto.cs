using OrderService.Shared.Dtos.OrderItemDtos;
using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.OrderDtos;

public record GetOrderShopDto(
    string Id,
    string UserId,
    OrderStatus OrderStatus,
    string Address,
    string City, 
    string Zip,
    DateTime CreatedAt,
    ICollection<GetOrderItemDto> OrderProducts
    );