using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.PaymentDtos;


public record PaymentResponseDto(string TransactionId, PaymentStatus Status);