using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Protos.GrpcOrderService;

namespace OrderService.Infrastructure.gRPC;

public class GrpcOrderService : OrderService.Shared.Protos.GrpcOrderService.OrderService.OrderServiceBase
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger _logger;

    public GrpcOrderService(IOrderRepository orderRepository, ILogger logger)
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
}