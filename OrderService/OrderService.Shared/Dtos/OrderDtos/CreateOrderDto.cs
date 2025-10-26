using OrderService.Shared.Dtos.OrderItemDtos;

namespace OrderService.Shared.Dtos.OrderDtos;


public record CreateOrderDto(
    string Address, 
    string City,
    int Zip,
    ICollection<CreateOrderItemDto> Products); 