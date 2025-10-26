using AutoMapper;
using OrderService.Core.Entities;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.OrderItemDtos;

namespace OrderService.Core.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderDto, Order>();
        CreateMap<Order, GetOrderDto>();
        CreateMap<OrderItem, GetOrderItemDto>();  
    }
}