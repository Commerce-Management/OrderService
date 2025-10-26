using OrderService.Shared.Dtos.Jwt;
using OrderService.Shared.Dtos.OrderDtos;

namespace OrderService.Core.Interfaces;

public interface IOrderStatusUpdatedEmail
{
    Task<string> GenerateEmailAsync(GetOrderDto order, string customerName);
}