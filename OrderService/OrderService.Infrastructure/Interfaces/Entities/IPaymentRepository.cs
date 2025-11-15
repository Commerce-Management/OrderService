using System.Collections.Generic;
using System.Threading.Tasks;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Interfaces.Base;

namespace OrderService.Infrastructure.Interfaces.Entities;


public interface IPaymentRepository : IRepository<PaymentTransaction>
{
    public Task<IEnumerable<PaymentTransaction>> GetAllPaymentTransactionsAsync();

}