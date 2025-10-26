using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using OrderService.Core.Interfaces;
using OrderService.Shared.Dtos.PaymentDtos;

namespace OrderService.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]

public class PaymentController(IPaymentService paymentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetPaymentTransactionDto>>> GetAllPaymentTransactions()
    {
        var result = await paymentService.GetAllPaymentTransactionsAsync();
        if (result.Any())
            return Ok(result);

        return NotFound("Payment transactions not found");
    }
}