
using OrderService.Shared.Dtos.OrderDtos;

namespace OrderService.Shared.Dtos.PaymentDtos;

public record CreateOrderRequest(CreateOrderDto Order, PaymentRequestDto PaymentRequest);