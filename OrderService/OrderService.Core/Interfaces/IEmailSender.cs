using OrderService.Shared.Dtos.EmailDtos;

namespace OrderService.Core.Interfaces;

public interface IEmailSender
{
    public Task SendEmailAsync(CreateEmailDto createEmailDto);
}