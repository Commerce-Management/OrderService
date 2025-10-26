namespace OrderService.Shared.Dtos.OrderDtos;


public class ProductSummary
{
    public string? Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}