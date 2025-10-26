using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.PaymentDtos;


public record GetPaymentTransactionDto(string OrderId, decimal Amount, DateTime TransactionDate, PaymentStatus Status);