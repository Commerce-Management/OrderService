using AutoMapper;
using OrderService.Core.Entities;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.OrderItemDtos;

namespace OrderService.Core.Profiles;

public class OrderProfile : Profile
{
    public OrderProfile()
    {
        CreateMap<CreateOrderDto, Order>()
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<Order, GetOrderDto>();
        CreateMap<OrderItem, GetOrderItemDto>();

        CreateMap<Order, GetOrderShopDto>()
        .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
        .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
        .ForMember(dest => dest.TrackingId, opt => opt.MapFrom(src => src.TrackingId ?? string.Empty))
        .ForMember(dest => dest.OrderStatus, opt => opt.MapFrom(src => src.Status))
        .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.OrderDate))
        .ForMember(dest => dest.OrderProducts, opt => opt.MapFrom(src => src.OrderItems));
    }
}
