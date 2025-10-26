using OrderService.Core.Entities;
using OrderService.Infrastructure.Interfaces.Base;

namespace OrderService.Infrastructure.Interfaces.Entities;

public interface IOrderItemRepository : IRepository<OrderItem>
{
    public Task CreateManyOrderItemsAsync(IEnumerable<OrderItem> orderItems);
}