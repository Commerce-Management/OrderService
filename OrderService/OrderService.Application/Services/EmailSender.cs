using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using OrderService.Core.Interfaces;
using OrderService.Shared.Dtos.EmailDtos;

namespace OrderService.Application.Services;

public class EmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly SmtpClient _smtpClient;

    public EmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
      
        _smtpClient = new SmtpClient(_configuration["Email:Host"])
        {
            Port = int.Parse(_configuration["Email:Port"]),
            Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"]),
            EnableSsl = true
        };
    }



    public async Task SendEmailAsync(CreateEmailDto emailDto)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_configuration["Email:Username"]),
            Subject = emailDto.Subject,
            Body = emailDto.Message,
            IsBodyHtml = true 
        };

        if (emailDto.Attachment != null)
        {
            mailMessage.Attachments.Add(emailDto.Attachment);
        }
        
        mailMessage.To.Add(emailDto.Email);

        await _smtpClient.SendMailAsync(mailMessage);
    }
   
}