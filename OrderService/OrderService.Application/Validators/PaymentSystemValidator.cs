using OrderService.Core.Interfaces;
using OrderService.Shared.Dtos.PaymentDtos;
using FluentValidation;

namespace OrderService.Application.Validators;

public class PaymentSystemValidator : AbstractValidator<PaymentRequestDto>
{
    private readonly IPaymentService _paymentService;

    public PaymentSystemValidator(IPaymentService paymentService)
    {
        _paymentService = paymentService;

        RuleFor(x => x.CardNumber)
            .NotEmpty()
            .WithMessage("Card number is required")
            .Matches(RegexPatterns.cardNumberPattern)
            .WithMessage("Valid Card Number format is required");

        RuleFor(x => x.CVV)
            .NotEmpty()
            .WithMessage("CVV is required")
            .Matches(RegexPatterns.cvvPattern)
            .WithMessage("Valid CVV is required");

        RuleFor(x => x.ExpirationDate)
            .NotEmpty()
            .WithMessage("Expiration Date is required")
            .Matches(RegexPatterns.expirationDatePattern)
            .WithMessage("Valid Expiration Date is required");

        RuleFor(x => x.CardHolderName)
            .NotEmpty()
            .WithMessage("Card Holder Name is required")
            .Matches(RegexPatterns.cardHolderNamePattern)
            .WithMessage("Valid Card Holder Name is required");
    }
}