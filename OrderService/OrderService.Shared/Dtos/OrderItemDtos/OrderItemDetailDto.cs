namespace OrderService.Shared.Dtos.OrderItemDtos;

public record OrderItemDetailDto(
    string Id,
    string ProductId,
    string ProductName,
    List<string> ImageUrls,
    string ShopId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);