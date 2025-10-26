using OrderService.Core.Entities;
using OrderService.Infrastructure.Models;

namespace OrderService.Infrastructure.Interfaces.Base;

public interface IRepository<T> where T : IEntity
{
    IQueryable<T> GetAll();
    public Task<PageResult<T>> GetPageAsync(int page, int limit, QueryParameters<T>? queryParameters = null);
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<T?> GetByIdAsync(Guid id);
    public Task<T> InsertAsync(T entity);
    public Task InsertManyAsync(IEnumerable<T> entities);
    
    void Update(T entity);
    void Delete(T entity);
    public Task<int> SaveChangesAsync();    
}