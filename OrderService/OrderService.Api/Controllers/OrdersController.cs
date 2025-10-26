using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.Core.Interfaces;
using OrderService.Shared.Dtos.Jwt;
using OrderService.Shared.Dtos.OrderDtos;
using OrderService.Shared.Dtos.PaymentDtos;
using Serilog;

namespace OrderService.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]

public class OrdersController(IOrderService orderService) : ControllerBase
{
    
    [HttpGet]
    [Authorize("SuperAdmin")]
    public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetAllOrders()
    {
        var result = await orderService.GetAllOrders();
        if (result.Any()) return Ok(result);

        return NotFound("Orders not found.");
    }

    [HttpGet("shop")]
    [Authorize("ShopOwner")]
    public async Task<ActionResult<IEnumerable<GetOrderShopDto>>> GetShopOrders([FromQuery] Guid shopId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
    {
        var result = await orderService.GetAllShopOrders(shopId, page, limit);
        if (result.Any()) return Ok(result);

        return NotFound($"Orders not found");
    }
    
    [HttpGet("user/{id:guid}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetAllUserOrders()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized();
        
        var result = await orderService.GetAllUserOrdersAsync(userId);
        if (result.Any()) return Ok(result);

        return NotFound("Orders not found.");
    }
    
    [HttpGet("order/{id:guid}")]
    [Authorize("ShopOwner")]
    public async Task<ActionResult<IEnumerable<GetOrderDto>>> GetOrderById(Guid id)
    {
        var result = await orderService.GetOrderByIdAsync(id);
        if (result != null) return Ok(result);

        return NotFound($"Order with {result.Id} not found.");
    }
    
    [HttpPost]
    [Authorize("ShopCustomer")]
    public async Task<ActionResult<CreateOrderRequest>> CreateOrder([FromBody] CreateOrderRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { Error = "Model not valid" });
    
        try
        {
            var result = await orderService.CreateOrderAsync(request.Order,request.PaymentRequest);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "PRODUCT_NOT_FOUND")
        {
            return NotFound(new { Error = "One or more products in the order were not found." });
        }
        catch (Exception ex) when (ex.Message == "STOCK_NOT_ENOUGH")
        {
            return BadRequest(new { Error = "Insufficient stock for one or more products." });
        }
        catch (Exception ex) when (ex.Message == "FAILED_TO_CREATE_PAYPAL_ORDER")
        {
            return BadRequest(new { Error = "Failed to create PayPal order." });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while creating order.");
            var innerExceptionMessage = ex.InnerException?.Message ?? "No inner exception";
            return StatusCode(500, new
            {
                Error = "An unexpected error occurred. Please try again later.",
                Details = ex.Message,  
                InnerException = innerExceptionMessage,
                StackTrace = ex.StackTrace 
            });
        }
    }

    [HttpPost("{trackingId}/capture")]
    public async Task<IActionResult> CaptureOrder(string trackingId, PaymentRequestDto paymentRequest)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userEmailClaim))
            return Unauthorized();

        var userDto = new UserDto(
            Name: userNameClaim,   // если вдруг name отсутствует
            Email: userEmailClaim
        );
        
        try
        {
            await orderService.CaptureOrderAsync(userDto, trackingId,paymentRequest);
            return Ok();
        }
        catch(ArgumentException ex) when (ex.Message == "ORDER_NOT_FOUND")
        {
            return NotFound(new { Error = "Order not found" });
        }
        catch(ArgumentException ex) when (ex.Message == "PRODUCT_NOT_FOUND")
        {
            return NotFound(new { Error = "Product not found" });
        }
        catch (ArgumentException ex) when (ex.Message == "STOCK_NOT_ENOUGH")
        {
            return BadRequest(new { Error = "Insufficient stock for one or more products." });
        }
        catch (ArgumentException ex) when (ex.Message == "FAILED_TO_PAYMENT_PAYPAL_ORDER")
        {
            return BadRequest(new { Error = "Failed to capture payment order." });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while capturing order.");
            return StatusCode(500, new
            {
                Error = "An unexpected error occurred. Please try again later.",
                Details = ex.Message, 
                StackTrace = ex.StackTrace  
            });
        }
    }
    
    
    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> UpdateOrder(Guid id, CreateOrderDto orderDto)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model not valid");

        var result = await orderService.UpdateOrderAsync(id, orderDto);
        if (result) return Ok();

        return BadRequest($"Order with ID: {id} could not be updated.");
    }

    [HttpPut("status-update")]
    [Authorize]
    public async Task<ActionResult> UpdateOrderStatus(UpdateOrderStatusDto updateOrderStatus)
    {
        if (!ModelState.IsValid)
            return BadRequest("Model not valid");
        
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var userEmailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(userEmailClaim))
            return Unauthorized();

        var userDto = new UserDto(
            Name: userNameClaim,   // если вдруг name отсутствует
            Email: userEmailClaim
        );
        
        var result = await orderService.UpdateOrderStatusAsync(userDto, updateOrderStatus);
        if (result) return Ok();
        
        return BadRequest($"Order status with Id: {updateOrderStatus.OrderId} could not be updated.");
    }
    
    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<ActionResult> DeleteOrder(Guid id)
    {
        var result = await orderService.DeleteOrderAsync(id);
        if (result) return Ok();

        return BadRequest($"Order with ID: {id} could not be deleted.");
    }
    
    [HttpGet("tracking/{trackingId}")]
    public async Task<ActionResult<GetOrderDto>> GetOrderByTrackingNumber(string trackingId)
    {
        var result = await orderService.GetOrderByTrackingNumberAsync(trackingId);
        if (result != null) return Ok(result);
        return NotFound($"Order with tracking {trackingId} not found.");
    }
    
    
}