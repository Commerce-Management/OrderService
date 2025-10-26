using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.PaymentDtos;


public record CreateOrderResponse(string Id, string TrackingId, decimal Amount, PaymentStatus Status);