using OrderService.Shared.Dtos;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.PaymentDtos;

namespace OrderService.Core.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDto> ProcessPaymentAsync(GetOrderDto order, PaymentRequestDto paymentRequest);                 
    Task<PaymentResponseDto> ProcessPaymentSubscriptionAsync(Guid subscriptionId, PaymentRequestDto paymentRequest); 
    Task<IEnumerable<GetPaymentTransactionDto>> GetAllPaymentTransactionsAsync();                                      
}