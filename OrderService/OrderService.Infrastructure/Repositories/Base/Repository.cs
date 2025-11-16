using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OrderService.Core.Entities;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Interfaces.Base;
using OrderService.Infrastructure.Models;

namespace OrderService.Infrastructure.Repositories.Base;

public class Repository<T> : IRepository<T> where T : class, IEntity
{
    private readonly OrderDbContext _context;
    protected readonly DbSet<T> Entities;
    private IRepository<T> _repositoryImplementation;

    public Repository(OrderDbContext context)
    {
        _context = context;
        Entities = _context.Set<T>();
    }
    
    public virtual async Task<PageResult<T>> GetPageAsync(int page, int limit, QueryParameters<T>? queryParameters = null)
    {
        IQueryable<T> query = Entities;

        if (queryParameters != null)
        {
            if (queryParameters.Filter != null)
                query = query.Where(queryParameters.Filter);

            if (queryParameters.Include != null)
                query = queryParameters.Include(query);
        }

        var offset = (page - 1) * limit;
        var totalItems = await query.CountAsync();

        var items = await query
            .Skip(offset)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync();

        return new PageResult<T>(limit, totalItems, items);
    }
    
    public virtual IQueryable<T> GetAll() => 
        Entities.AsNoTracking();
    
    public virtual async Task<IEnumerable<T>> GetAllAsync() => 
        await GetAll().ToListAsync();
    
    public virtual async Task<T?> GetByIdAsync(Guid id) =>
        await GetAll().SingleOrDefaultAsync(s => s.Id == id);

    public virtual async Task<T> InsertAsync(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        var entityItem = await Entities.AddAsync(entity);
        
        await _context.SaveChangesAsync();
        
        return (await Entities.SingleOrDefaultAsync(s => s.Id == entityItem.Entity.Id))!;
    }

    public async Task InsertManyAsync(IEnumerable<T> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);

        await Entities.AddRangeAsync(entities);

        await _context.SaveChangesAsync();
    }

    public void Update(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Update(entity);
    }

    public void Delete(T entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        Entities.Remove(entity);
    }

    public async Task<int> SaveChangesAsync() =>
        await _context.SaveChangesAsync();
}