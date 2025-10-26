using OrderService.Core.Entities;
using OrderService.Shared.Dtos.Jwt;

namespace OrderService.Core.Interfaces;

public interface IOrderConfirmationEmail
{
    Task<string> GenerateEmailAsync(Order order, string customerName);
}