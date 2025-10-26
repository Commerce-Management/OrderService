using Microsoft.EntityFrameworkCore;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Interfaces.Entities;
using OrderService.Infrastructure.Repositories.Base;

namespace OrderService.Infrastructure.Repositories.Entities;

public class PaymentRepository(OrderDbContext context) : Repository<PaymentTransaction>(context), IPaymentRepository
{
    private IQueryable<PaymentTransaction> GetPaymentQuery() =>
        Entities
            .Include(p => p.Order)
            .AsNoTracking();

    public async Task<IEnumerable<PaymentTransaction>> GetAllPaymentTransactionsAsync() =>
        await Entities
            .Include(p => p.Order)          
            .AsNoTracking()
            .ToListAsync();
         

    public async Task<IEnumerable<PaymentTransaction>> GetByOrderIdAsync(Guid orderId) =>
        await Entities
            .AsNoTracking()
            .Where(pt => pt.OrderId == orderId)                 
            .OrderByDescending(pt => pt.TransactionDate)       
            .ToListAsync();
    
}