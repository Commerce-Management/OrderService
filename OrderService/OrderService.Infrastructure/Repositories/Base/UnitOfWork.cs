using Microsoft.EntityFrameworkCore.Storage;
using OrderService.Infrastructure.Context;
using OrderService.Infrastructure.Interfaces.Base;

namespace OrderService.Infrastructure.Repositories.Base;

public class UnitOfWork(OrderDbContext context) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    
    public async Task<bool> SaveChangesAsync()
    {
        return await context.SaveChangesAsync() > 1;
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is null) throw new ArgumentException("Transaction has not started");
        await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is null) throw new ArgumentException("Transaction has not started");
        await _transaction.RollbackAsync();
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        await BeginTransactionAsync();

        try
        {
            await action();
            await CommitTransactionAsync();
            await SaveChangesAsync();
        }
        catch 
        { 
            await RollbackTransactionAsync();
            throw;
        }
    }
}