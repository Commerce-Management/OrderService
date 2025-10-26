namespace OrderService.Shared.Dtos.PaymentDtos;

public record PaymentRequestDto(string CardNumber, string ExpirationDate, string CVV, string CardHolderName, decimal Amount);