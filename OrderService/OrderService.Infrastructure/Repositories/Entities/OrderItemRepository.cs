using OrderService.Core.Entities;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Infrastructure.Repositories.Base;

namespace OrderService.Infrastructure.Repositories.Entities;

public class OrderItemRepository(OrderDbContext context) : Repository<OrderItem>(context), IOrderItemRepository
{
    public async Task CreateManyOrderItemsAsync(IEnumerable<OrderItem> orderItems)
    {
        await Entities.AddRangeAsync(orderItems);
        await base.SaveChangesAsync();
    }
}