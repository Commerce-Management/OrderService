using OrderService.Shared.Dtos;
using OrderService.Shared.Dtos.Jwt;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.PaymentDtos;

namespace OrderService.Core.Interfaces;

public interface IOrderService
{
    public Task<IEnumerable<GetOrderDto>> GetAllOrders();
    public Task<IEnumerable<GetOrderDto>> GetAllUserOrdersAsync(Guid userId);
    public Task<IEnumerable<GetOrderShopDto>> GetAllShopOrders(Guid shopId, int page, int limit);
    public Task<GetOrderDto?> GetOrderByIdAsync(Guid id);
    public Task<GetOrderDto?> GetOrderByTrackingNumberAsync(string trackingNumber);
    public Task<CreateOrderResponse> CreateOrderAsync(CreateOrderDto orderDto, PaymentRequestDto paymentRequest, Guid userId);
    public Task CaptureOrderAsync(UserDto user, string trackingId, PaymentRequestDto paymentRequest);
    public Task<bool> UpdateOrderAsync(Guid id, CreateOrderDto orderDto);
    
    public Task<bool> UpdateOrderStatusAsync(UserDto user, UpdateOrderStatusDto updateOrderStatus);
    public Task<bool> DeleteOrderAsync(Guid id);
}