using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Enums;
using OrderService.Shared.Protos.GrpcOrderService;


namespace OrderService.Infrastructure.gRPC;

public class GrpcOrderService : OrderService.Shared.Protos.GrpcOrderService.OrderService.OrderServiceBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GrpcOrderService> _logger;

    public GrpcOrderService(IOrderRepository orderRepository, ILogger<GrpcOrderService> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public override async Task<GetAllUserOrderResponse> GetAllUserOrders(
        GetAllUserOrderRequest request,
        ServerCallContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.UserId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));

        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId invalid"));

        var orders = await _orderRepository.GetAllUserOrdersAsync(userId);
        var resp = new GetAllUserOrderResponse();

        if (orders is null || !orders.Any())
            return resp;

        // Distinct on (userId, productId)
        var seen = new HashSet<(Guid UserId, Guid ProductId)>();

        foreach (var o in orders)
        {
            foreach (var item in o.OrderItems)
            {
                var key = (o.UserId, item.ProductId);
                if (seen.Add(key))
                {
                    resp.Items.Add(new OrderProduct
                    {
                        ProductId = item.ProductId.ToString()
                    });
                }
            }
        }

        return resp;
    }
    
    
    public override async Task<GetOrderStatusByProductIdAndUserIdResponse> GetOrderStatusByProductIdAndUserId(
        GetOrderStatusByProductIdAndUserIdRequest request,
        ServerCallContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ProductId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "ProductId is required"));

        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));

        if (!Guid.TryParse(request.ProductId, out var productId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid ProductId"));

        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId"));

        try
        {
            var order = await _orderRepository.GetOrderByProductIdAsync(productId, userId);

            if (order == null)
            {
                return new GetOrderStatusByProductIdAndUserIdResponse
                {
                    Exists = false,
                    Status = string.Empty
                };
            }

            return new GetOrderStatusByProductIdAndUserIdResponse
            {
                Exists = true,
                Status = order.Status.ToString()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetOrderStatusByProductId failed");
            throw new RpcException(new Status(StatusCode.Internal, $"Internal server error: {ex.Message}"));
        }
    }
    
    public override async Task<GetPurchasedProductIdsByUserIdResponse> GetPurchasedProductIdsByUserId(
        GetPurchasedProductIdsByUserIdRequest request,
        ServerCallContext context)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "UserId is required"));

        if (!Guid.TryParse(request.UserId, out var userId))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid UserId"));

        try
        {
            var orders = await _orderRepository.GetAllUserOrdersAsync(userId);
            var response = new GetPurchasedProductIdsByUserIdResponse();

            if (orders?.Any() == true)
            {
                var purchasedProductIds = orders
                    .Where(o => 
                        o.Status == OrderStatus.Delivered || 
                        o.Status == OrderStatus.Shipped ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.OnHold ||
                        o.Status == OrderStatus.Pending) // ← Статусы для ИСКЛЮЧЕНИЯ из рекомендаций
                    .SelectMany(o => o.OrderItems)
                    .Select(item => item.ProductId.ToString())
                    .Distinct()
                    .ToList();

                response.ProductIds.AddRange(purchasedProductIds);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPurchasedProductIdsByUserId failed for user {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, $"Internal error: {ex.Message}"));
        }
    }
}