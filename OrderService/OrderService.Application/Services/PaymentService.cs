using AutoMapper;
using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using OrderService.Infrastructure.Interfaces.Base;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.PaymentDtos;
using OrderService.Shared.Enums;

namespace OrderService.Application.Services;

public class PaymentService(
    IUnitOfWork unitOfWork,
    IPaymentRepository paymentRepository,
    IMapper mapper
) : IPaymentService
{
    public async Task<PaymentResponseDto> ProcessPaymentAsync(GetOrderDto order, PaymentRequestDto paymentRequest)
    {
        ValidatePaymentDetail(paymentRequest);

        if (!Guid.TryParse(order.Id, out var orderId))
            throw new ArgumentException("Invalid OrderId format.");

        var transaction = new PaymentTransaction
        {
            OrderId = orderId,
            Amount = paymentRequest.Amount,
            Status = PaymentStatus.Succeeded,
            TransactionDate = DateTime.UtcNow
        };

        await paymentRepository.InsertAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        return new PaymentResponseDto(
            transaction.TransactionReference,
            transaction.Status
        );
    }

    public async Task<PaymentResponseDto> ProcessPaymentSubscriptionAsync(Guid subscriptionId, PaymentRequestDto paymentRequest)
    {
        ValidatePaymentDetail(paymentRequest);

        var transaction = new PaymentTransaction
        {
            Amount = paymentRequest.Amount,
            Status = PaymentStatus.Succeeded,
        };

        await paymentRepository.InsertAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        return new PaymentResponseDto(
            transaction.TransactionReference,
            transaction.Status
        );
    }

    public async Task<IEnumerable<GetPaymentTransactionDto>> GetAllPaymentTransactionsAsync() =>
        mapper.Map<IEnumerable<GetPaymentTransactionDto>>(
            await paymentRepository.GetAllPaymentTransactionsAsync()
        );

    private void ValidatePaymentDetail(PaymentRequestDto paymentRequest)
    {
        if (string.IsNullOrEmpty(paymentRequest.CardNumber) ||
            string.IsNullOrEmpty(paymentRequest.ExpirationDate) ||
            string.IsNullOrEmpty(paymentRequest.CVV))
        {
            throw new ArgumentException("Invalid payment details.");
        }

        if (!IsValidCardNumber(paymentRequest.CardNumber))
            throw new ArgumentException("Invalid card number.");

        if (!IsValidExpirationDate(paymentRequest.ExpirationDate))
            throw new ArgumentException("Invalid expiration date.");
    }

    private bool IsValidCardNumber(string cardNumber)
    {
        int sum = 0;
        bool alternate = false;
        for (int i = cardNumber.Length - 1; i >= 0; i--)
        {
            int n = int.Parse(cardNumber[i].ToString());
            if (alternate)
            {
                n *= 2;
                if (n > 9) n -= 9;
            }
            sum += n;
            alternate = !alternate;
        }
        return (sum % 10) == 0;
    }

    private bool IsValidExpirationDate(string expirationDate)
    {
        if (DateTime.TryParseExact(expirationDate, "MM/yy", null, System.Globalization.DateTimeStyles.None, out var date))
            return date > DateTime.UtcNow;
        return false;
    }
}