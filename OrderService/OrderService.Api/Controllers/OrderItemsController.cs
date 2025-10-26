using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Infrastructure.Context;
using OrderService.Shared.Protos.GrpcProductService;
using OrderService.Shared.Dtos.OrderItemDtos;

namespace OrderService.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]

public class OrderItemsController : ControllerBase
{
    private readonly OrderDbContext _context;
    private readonly ProductService.ProductServiceClient _productServiceClient;

    public OrderItemsController(OrderDbContext context, ProductService.ProductServiceClient productServiceClient)
    {
        _context = context;
        _productServiceClient = productServiceClient;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderItemDetailDto>>> GetOrderItems()
    {
        var orderItems = await _context.OrderItems.ToListAsync();

        if (!orderItems.Any())
            return Ok(new List<OrderItemDetailDto>());

        var productIds = orderItems
            .Select(i => i.ProductId.ToString())
            .Distinct()
            .ToList();

        var productResponse = await _productServiceClient.GetProductsByIdsAsync(new GetProductsByIdsRequest
        {
            ProductIds = { productIds }
        });

        var productMap = productResponse.Products
            .Where(p => Guid.TryParse(p.Id, out _))
            .ToDictionary(p => Guid.Parse(p.Id), p => p);

        var result = orderItems.Select(oi =>
        {
            productMap.TryGetValue(oi.ProductId, out var product);

            var name = product?.Name ?? "Unknown Product";
            var images = product?.ImageUrls?.ToList() ?? new List<string>();
            var shopId = product?.ShopId ?? string.Empty;
            var unitPrice = product != null ? (decimal)product.Price / 100 : oi.UnitPrice;
            var totalPrice = unitPrice * oi.Quantity;

            return new OrderItemDetailDto(
                oi.Id.ToString(),
                oi.ProductId.ToString(),
                name,
                images,
                shopId,
                oi.Quantity,
                unitPrice,
                totalPrice
            );
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderItemDetailDto>> GetOrderItem(Guid id)
    {
        var orderItem = await _context.OrderItems.FirstOrDefaultAsync(i => i.Id == id);

        if (orderItem == null)
            return NotFound();

        var response = await _productServiceClient.GetProductsByIdsAsync(new GetProductsByIdsRequest
        {
            ProductIds = { orderItem.ProductId.ToString() }
        });

        var product = response.Products.FirstOrDefault();
        var name = product?.Name ?? "Unknown Product";
        var images = product?.ImageUrls?.ToList() ?? new List<string>();
        var shopId = product?.ShopId ?? string.Empty;
        var unitPrice = product != null ? (decimal)product.Price / 100 : orderItem.UnitPrice;
        var totalPrice = unitPrice * orderItem.Quantity;

        return Ok(new OrderItemDetailDto(
            orderItem.Id.ToString(),
            orderItem.ProductId.ToString(),
            name,
            images,
            shopId,
            orderItem.Quantity,
            unitPrice,
            totalPrice
        ));
    }
}
