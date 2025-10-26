using AutoMapper;
using OrderService.Core.Entities;
using OrderService.Shared.Dtos.OrderItemDtos;

namespace OrderService.Core.Profiles;

public class OrderItemProfile : Profile
{
    public OrderItemProfile()
    {
        CreateMap<CreateOrderItemDto, OrderItem>();
        CreateMap<OrderItem, GetOrderItemDto>();
    }
}