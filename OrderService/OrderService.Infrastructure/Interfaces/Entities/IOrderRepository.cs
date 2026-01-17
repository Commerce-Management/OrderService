using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Interfaces.Base;
using OrderService.Infrastructure.Models;

namespace OrderService.Infrastructure.Interfaces.Entities;


public interface IOrderRepository : IRepository<Order>
{
    public Task<Order?> GetOrderByIdAsync(Guid id);
    public Task<PageResult<Order>> GetAllShopOrdersAsync(Guid shopId, int page, int limit);
    public Task<IEnumerable<Order>> GetAllUserOrdersAsync(Guid userId);
    public Task<IEnumerable<Order>> GetAllOrdersByDate(DateTime startDate, DateTime endDate, Func<IQueryable<Order>, IQueryable<Order>>? additionalQuery = null);
    public Task<Order?> GetOrderByTrackingNumberAsync(string trackingNumber);
    
    public Task<Order?> GetOrderByProductIdAsync(Guid productId, Guid userId);

}