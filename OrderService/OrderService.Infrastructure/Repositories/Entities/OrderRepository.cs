using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

    public async Task<IEnumerable<Order>> GetAllUserOrdersAsync(Guid userId, int page, int limit)
    {
        if (page <= 0) page = 1;
        if (limit <= 0) limit = 10;

        var skip = (page - 1) * limit;

        return await Entities
            .Where(o => o.UserId == userId
                        && o.Status != OrderStatus.PaymentFailed
                        && o.Status != OrderStatus.Pending)
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllPaginatedAsync(int page, int limit)
    {
        if (page <= 0) page = 1;
        if (limit <= 0) limit = 10;

        var skip = (page - 1) * limit;

        return await Entities
            .Include(o => o.OrderItems)
            .OrderByDescending(o => o.OrderDate)
            .Skip(skip)
            .Take(limit)
            .ToListAsync();
    }


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
    
    
    public async Task<Order?> GetOrderByProductIdAsync(Guid productId, Guid userId) =>
        await Entities
            .Include(o => o.OrderItems)
            .Where(o =>
                o.UserId == userId &&   
                o.OrderItems.Any(i => i.ProductId == productId) &&
                o.Status != OrderStatus.Cancelled &&
                o.Status != OrderStatus.PaymentFailed)
            .OrderByDescending(o => o.OrderDate)
            .FirstOrDefaultAsync();

}