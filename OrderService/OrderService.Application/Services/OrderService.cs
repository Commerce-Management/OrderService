using AutoMapper;
using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using OrderService.Infrastructure.Interfaces.Base;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Shared.Dtos.EmailDtos;
using OrderService.Shared.Dtos.Jwt;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.PaymentDtos;
using OrderService.Shared.Enums;
using OrderService.Shared.Protos.GrpcProductService;
using OrderService.Shared.Protos.GrpcShopService;


using Serilog;

namespace OrderService.Application.Services;

public class OrderService(
    IUnitOfWork unitOfWork,
    IOrderRepository orderRepository,
    IOrderItemRepository orderItemRepository,
    ProductService.ProductServiceClient productServiceClient,
    ShopService.ShopServiceClient shopServiceClient,
    IMapper mapper,
    IEmailSender emailSender,
    IPaymentService paymentService,
    IOrderConfirmationEmail orderConfirmationEmail
) : IOrderService
{
    public async Task<IEnumerable<GetOrderDto>> GetAllOrders() =>
        mapper.Map<IEnumerable<GetOrderDto>>(await orderRepository.GetAllAsync());
    
    public async Task<IEnumerable<GetOrderShopDto>> GetAllShopOrders(Guid shopId, int page, int limit)
    {
        return mapper.Map<IEnumerable<GetOrderShopDto>>(await orderRepository.GetAllShopOrdersAsync(shopId, page, limit));
    }

    public async Task<IEnumerable<GetOrderDto>> GetAllUserOrdersAsync(Guid userId) =>
        mapper.Map<IEnumerable<GetOrderDto>>(await orderRepository.GetAllUserOrdersAsync(userId));

    public async Task<GetOrderDto?> GetOrderByIdAsync(Guid orderId) =>
        mapper.Map<GetOrderDto?>(await orderRepository.GetOrderByIdAsync(orderId));

    public async Task<GetOrderDto?> GetOrderByTrackingNumberAsync(string trackingNumber) =>
        mapper.Map<GetOrderDto?>(await orderRepository.GetOrderByTrackingNumberAsync(trackingNumber));

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderDto orderDto, PaymentRequestDto paymentRequest)
    {
        var (orderSummary, productsById) = await ValidateAndSummarizeOrder(orderDto);
        Log.Information("Order summary calculated. TotalPrice: {TotalPrice}", orderSummary.TotalPrice);

        var trackingId = $"{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";
        Log.Information("Generated TrackingId: {TrackingId}", trackingId);

        var savedOrder = await SaveOrder(orderDto, trackingId, productsById);
        Log.Information("Order saved with Id: {OrderId} and TotalAmount: {TotalAmount}", savedOrder.Id,
            savedOrder.TotalAmount);

        var paymentResponse =
            await paymentService.ProcessPaymentAsync(mapper.Map<GetOrderDto>(savedOrder), paymentRequest);
        Log.Information("Payment processed. Status: {PaymentStatus}", paymentResponse.Status);

        if (paymentResponse.Status == PaymentStatus.Succeeded)
        {
            savedOrder.Status = OrderStatus.Confirmed;

            orderRepository.Update(savedOrder);
            await orderRepository.SaveChangesAsync();
            Log.Information("Order status updated to Confirmed for OrderId: {OrderId}", savedOrder.Id);
        }
        else
        {
            Log.Error("Payment failed for OrderId: {OrderId}", savedOrder.Id);
            throw new Exception("PAYMENT_FAILED");
        }

        var response = new CreateOrderResponse(
            Id: savedOrder.Id.ToString(),
            TrackingId: savedOrder.TrackingId,
            Amount: savedOrder.TotalAmount,
            Status: paymentResponse.Status
        );

        Log.Information("CreateOrderAsync completed. Response: {@Response}", response);
        return response;
    }


    public async Task CaptureOrderAsync(UserDto user, string trackingId, PaymentRequestDto paymentRequest)
    {
        var order = await orderRepository.GetOrderByTrackingNumberAsync(trackingId)
                    ?? throw new ArgumentException("ORDER_NOT_FOUND");
        
        if (order.Status == OrderStatus.Confirmed)
            return;

        var products = await productServiceClient.GetProductsByIdsAsync(new GetProductsByIdsRequest()
        {
            ProductIds = { order.OrderItems.Select(i => i.ProductId.ToString()) }
            
        });
        
        var productsById = products.Products
            .Where(p => Guid.TryParse(p.Id, out _))
            .ToDictionary(p => Guid.Parse(p.Id), p => p);
        
        var orderProducts = order.OrderItems.ToDictionary(p => p.ProductId);

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            await productServiceClient.UpdateProductStockAsync(new UpdateProductStockRequest()
            {
                Updates =
                {
                    order.OrderItems.Select(i => new StockUpdate
                    {
                        ProductId = i.ProductId.ToString(),
                        Quantity = i.Quantity
                    })
                }
            });
            
            var orderDto = mapper.Map<GetOrderDto>(order);

            var response = await paymentService.ProcessPaymentAsync(orderDto, paymentRequest);

            if (response.Status.Equals(PaymentStatus.Failed))
                throw new ArgumentException("Failed to payment order");

            order.Status = OrderStatus.Confirmed;
            orderRepository.Update(order);

            var email = await orderConfirmationEmail.GenerateEmailAsync(order, user.Name);
            await emailSender.SendEmailAsync(new CreateEmailDto(
                Email: user.Email!,
                Subject: $"Order Confirmation #{order.TrackingId}",
                Message: email
            ));
        });
    }

    public async Task<bool> UpdateOrderAsync(Guid id, CreateOrderDto orderDto)
    {
        var orderEntity = await orderRepository.GetOrderByIdAsync(id);

        if (orderEntity is null)
            return false;

        mapper.Map(orderDto, orderEntity);
        orderRepository.Update(orderEntity);

        return await orderRepository.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateOrderStatusAsync(UserDto user, UpdateOrderStatusDto updateOrderStatus)
    {
        var orderEntity = await orderRepository.GetOrderByIdAsync(Guid.Parse(updateOrderStatus.OrderId));

        if (orderEntity is null)
            return false;

        orderEntity.Status = updateOrderStatus.Status;
        orderRepository.Update(orderEntity);

        var email = await orderConfirmationEmail.GenerateEmailAsync(orderEntity, user.Name);
        await emailSender.SendEmailAsync(new CreateEmailDto(
            Email: user.Email!,
            Subject: $"Your Order #{orderEntity.TrackingId}, {orderEntity.Status}",
            Message: email
        ));

        return await orderRepository.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteOrderAsync(Guid orderId)
    {
        var order = await orderRepository.GetOrderByIdAsync(orderId);

        if (order is null)
            return false;

        orderRepository.Delete(order);

        return await orderRepository.SaveChangesAsync() > 0;
    }

    
    
    private static decimal MinorToDecimal(long minor) => decimal.Divide(minor, 100m);
    private async Task<(OrderSummary summary, Dictionary<Guid, ProductBrief> productsById)> ValidateAndSummarizeOrder(CreateOrderDto orderDto)
    {
        var orderSummary = new OrderSummary();

        var productIds = orderDto.Products
            .Select(p => Guid.TryParse(p.ProductId, out var g) ? g : Guid.Empty)
            .Where(g => g != Guid.Empty)
            .Distinct()
            .ToArray();
        
        if (productIds.Length == 0)
            throw new Exception("NO_VALID_PRODUCTS");

        var products = await productServiceClient.GetProductsByIdsAsync(new GetProductsByIdsRequest()
        {
            ProductIds = { productIds.Select(x => x.ToString()) }
        });
        
        var productsById = products.Products
            .Where(p => Guid.TryParse(p.Id, out _))
            .ToDictionary(p => Guid.Parse(p.Id), p => p);
        

        foreach (var orderProduct in orderDto.Products)
        {
            if (!Guid.TryParse(orderProduct.ProductId, out var productId))
                throw new Exception($"Invalid ProductId: {orderProduct.ProductId}");

            if (!productsById.TryGetValue(productId, out var product))
                throw new Exception($"Product not found: {orderProduct.ProductId}");

            if (product.StockQuantity < orderProduct.Quantity)
                throw new Exception($"Not enough stock for product: {product.Name}");

            Log.Information($"Product: {product.Name}, Price: {product.Price}, Quantity: {orderProduct.Quantity}");

            var unitPrice = MinorToDecimal(product.Price);
            orderSummary.TotalPrice += unitPrice * orderProduct.Quantity;
            orderSummary.Products.Add(new ProductSummary
            {
                Name = product.Name,
                Price = product.Price,
                Quantity = orderProduct.Quantity
            });
        }

        // Log.Information($"Total order price: {orderSummary.TotalPrice}, " +
        //                 $"Order products: {JsonConvert.SerializeObject(orderSummary.Products)}");

        return (orderSummary, productsById);
    }

    private async Task<Order> SaveOrder(CreateOrderDto orderDto, string trackingId,  Dictionary<Guid, ProductBrief> productsById)
    {
        // Log.Information("SaveOrder started for UserId: {UserId} with TrackingId: {TrackingId}", orderDto.UserId,
        //     trackingId);

        var orderEntity = mapper.Map<Order>(orderDto);
        orderEntity.TrackingId = trackingId;
        orderEntity.Status = OrderStatus.Pending;

        Order savedOrder = null;

        await unitOfWork.ExecuteTransactionAsync(async () =>
        {
            savedOrder = await orderRepository.InsertAsync(orderEntity);
            Log.Information("Order inserted with Id: {OrderId}", savedOrder.Id);

            var orderItemEntities = orderDto.Products.Select(item =>
            {
                var prodId = Guid.Parse(item.ProductId);
                var product = productsById[prodId]; 
                var unitPrice = MinorToDecimal(product.Price) * item.Quantity;
                
                var shopId = Guid.TryParse(product.ShopId, out var sId) ? sId : Guid.Empty;
                if (shopId == Guid.Empty)
                    throw new Exception($"Invalid ShopId for product {product.Id}");
                
                Log.Information(
                    "Creating OrderItem for ProductId: {ProductId}, Quantity: {Quantity}, UnitPrice: {UnitPrice}",
                    prodId, item.Quantity, unitPrice);
                
                
                return new OrderItem
                {
                    OrderId = savedOrder.Id,
                    ProductId = prodId,
                    ShopId = shopId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice
                };
            }).ToList();

            Log.Information("Created {Count} OrderItems", orderItemEntities.Count);

            await orderItemRepository.CreateManyOrderItemsAsync(orderItemEntities);
            
            Log.Information("OrderItems saved for OrderId: {OrderId}", savedOrder.Id);

            savedOrder.TotalAmount = orderItemEntities.Sum(op => op.UnitPrice);
            Log.Information("Calculated TotalAmount: {TotalAmount} for OrderId: {OrderId}", savedOrder.TotalAmount,
                savedOrder.Id);

            savedOrder.OrderItems = orderItemEntities;

            orderRepository.Update(savedOrder);
            await orderRepository.SaveChangesAsync();
            Log.Information(
                "Order updated with TotalAmount and OrderItems for OrderId: {OrderId}. OrderItems count: {Count}",
                savedOrder.Id, savedOrder.OrderItems.Count);
        });

        Log.Information("SaveOrder completed for OrderId: {OrderId}", savedOrder.Id);
        return savedOrder;
    }
  
}