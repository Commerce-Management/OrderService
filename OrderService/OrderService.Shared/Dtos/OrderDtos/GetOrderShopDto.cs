using OrderService.Shared.Dtos.OrderItemDtos;
using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.OrderDtos;

public record GetOrderShopDto
{
    public string Id { get; init; }
    public string UserId { get; init; }
    public string TrackingId { get; init; }
    public OrderStatus OrderStatus { get; init; }
    public string Address { get; init; }
    public string City { get; init; }
    public string Zip { get; init; }
    public DateTime CreatedAt { get; init; }
    public ICollection<GetOrderItemDto> OrderProducts { get; init; }
}