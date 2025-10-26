using System.Net.Mail;

namespace OrderService.Shared.Dtos.EmailDtos;

public record CreateEmailDto(string Email, string Subject, string Message, Attachment? Attachment=null);