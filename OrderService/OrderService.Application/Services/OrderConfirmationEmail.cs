using System.Text;
using OrderService.Core.Entities;
using OrderService.Core.Interfaces;
using OrderService.Shared.Protos.GrpcProductService;

namespace OrderService.Application.Services;

public class OrderConfirmationEmail : IOrderConfirmationEmail
{
    private readonly ProductService.ProductServiceClient _productServiceClient;

    public OrderConfirmationEmail(ProductService.ProductServiceClient productServiceClient)
    {
        _productServiceClient = productServiceClient;
    }

    private static decimal MinorToDecimal(long minor) => decimal.Divide(minor, 100m);

    public async Task<string> GenerateEmailAsync(Order order, string customerName)
    {
        var templatePath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "order_confirmation.html");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template not found at {templatePath}");

        var template = File.ReadAllText(templatePath);

        // ---- gRPC: Получаем данные продуктов ----
        var productIds = order.OrderItems
            .Select(i => i.ProductId.ToString())
            .Distinct()
            .ToList();

        var productResponse = await _productServiceClient.GetProductsByIdsAsync(
            new GetProductsByIdsRequest { ProductIds = { productIds } });

        var productsById = productResponse.Products
            .Where(p => Guid.TryParse(p.Id, out _))
            .ToDictionary(p => Guid.Parse(p.Id), p => p);

        // ---- Формируем HTML строку ----
        var orderItemsHtml = new StringBuilder();

        foreach (var item in order.OrderItems)
        {
            productsById.TryGetValue(item.ProductId, out var product);

            var name = product?.Name ?? "Product";
            var priceDecimal = product != null ? MinorToDecimal(product.Price) : 0m;

            orderItemsHtml.Append($@"
                <tr>
                    <td style=""padding: 10px; border-bottom: 1px solid #eee;"">{name}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #eee; text-align: right;"">{item.Quantity}</td>
                    <td style=""padding: 10px; border-bottom: 1px solid #eee; text-align: right;"">{priceDecimal:0.00} ₼</td>
                </tr>");
        }

        // ---- Заполняем шаблон ----
        return template
            .Replace("{CustomerName}", customerName)
            .Replace("{OrderNumber}", order.TrackingId)
            .Replace("{OrderDate}", order.OrderDate.ToString("MMMM dd, yyyy"))
            .Replace("{OrderItems}", orderItemsHtml.ToString())
            .Replace("{TotalAmount}", $"{order.TotalAmount:0.00} ₼")
            .Replace("{ShippingAddress}", order.Address);
    }
}
