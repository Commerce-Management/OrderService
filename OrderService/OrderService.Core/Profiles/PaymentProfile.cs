using AutoMapper;
using OrderService.Core.Entities;
using OrderService.Shared.Dtos.PaymentDtos;

namespace OrderService.Core.Profiles;

public class PaymentProfile : Profile
{
    public PaymentProfile()
    {
        CreateMap<PaymentTransaction, GetPaymentTransactionDto>()
            .ForMember(dest => dest.OrderId,
                opt => opt.MapFrom(src => src.OrderId.ToString()));

        CreateMap<PaymentTransaction, PaymentTransactionDto>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.Id.ToString()));
    }
}