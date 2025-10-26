using Microsoft.EntityFrameworkCore;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Infrastructure.Models;
using OrderService.Infrastructure.Repositories.Base;
using OrderService.Shared.Enums;
using RestSharp;

namespace OrderService.Infrastructure.Repositories.Entities;

public class OrderRepository(OrderDbContext context) : Repository<Order>(context), IOrderRepository
{
    public async Task<Order?> GetOrderByIdAsync(Guid id) =>
        await Entities
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<PageResult<Order>> GetAllShopOrdersAsync(Guid shopId, int page, int limit)
    {
        return await base.GetPageAsync(page, limit, new QueryParameters<Order>
        {
            Filter = o => o.OrderItems.Any(op => op.ShopId == shopId),
            Include = query => query.Include(o => o.OrderItems.Where(op => op.ShopId == shopId))
        });
    }
    
    public async Task<IEnumerable<Order>> GetAllUserOrdersAsync(Guid userId) =>
        await Entities
            .Where(o => o.UserId == userId && o.Status != OrderStatus.PaymentFailed && o.Status != OrderStatus.Pending)
            .Include(o => o.OrderItems)
            .ToListAsync();

    public async Task<IEnumerable<Order>> GetAllOrdersByDate(DateTime startDate, DateTime endDate, Func<IQueryable<Order>, IQueryable<Order>>? additionalQuery = null)
    {
        var baseQuery = Entities.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
        
        var finalQuery = additionalQuery != null ? additionalQuery(baseQuery) : baseQuery;
        
        return await finalQuery.Include(o => o.OrderItems).ToListAsync();
    }

    public async Task<Order?> GetOrderByTrackingNumberAsync(string trackingNumber) =>
        await Entities
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.TrackingId == trackingNumber);
}