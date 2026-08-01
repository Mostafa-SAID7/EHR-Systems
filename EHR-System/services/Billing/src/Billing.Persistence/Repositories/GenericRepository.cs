using Microsoft.EntityFrameworkCore;
using EHRPlatform.Services.Billing.Domain.Entities;

namespace EHRPlatform.Services.Billing.Persistence.Repositories;

/// <summary>
/// Generic repository for data access operations.
/// Provides basic CRUD and query functionality for all entities.
/// </summary>
public class GenericRepository<TEntity> where TEntity : class
{
    protected readonly BillingContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public GenericRepository(BillingContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Func<TEntity, bool> predicate, CancellationToken cancellationToken = default)
    {
        return DbSet.AsNoTracking().FirstOrDefault(x => predicate(x));
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        await DbSet.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public void RemoveRange(IEnumerable<TEntity> entities)
    {
        DbSet.RemoveRange(entities);
    }

    public IQueryable<TEntity> AsQueryable()
    {
        return DbSet.AsQueryable();
    }
}
