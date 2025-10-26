namespace OrderService.Application.Validators;

public class RegexPatterns
{
    public const string cardNumberPattern = @"^(?:4[0-9]{12}(?:[0-9]{3})?|5[1-5][0-9]{14}|3[47][0-9]{13}|3(?:0[0-5]|[68][0-9])[0-9]{11}|6(?:011|5[0-9]{2})[0-9]{12}|(?:2131|1800|35\d{3})\d{11})$";
    public const string cvvPattern = @"^\d{3}$";
    public const string expirationDatePattern = @"^(0[1-9]|1[0-2])\/\d{2}$"; 
    public const string cardHolderNamePattern = @"^[A-Za-z\s]{2,50}$";
}