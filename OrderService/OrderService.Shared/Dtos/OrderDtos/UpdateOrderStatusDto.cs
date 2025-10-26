using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.OrderDtos;

public record UpdateOrderStatusDto(string OrderId, OrderStatus Status);