
using OrderService.Shared.Enums;

namespace OrderService.Shared.Dtos.PaymentDtos;

public record PaymentTransactionDto(
    string Id,
    decimal Amount,
    DateTime TransactionDate,
    PaymentStatus Status,
    string TransactionReference
);