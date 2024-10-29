using System.Linq.Expressions;
using AspNetCore_Template.model.entity.abs;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore_Template.repository.abs;

public interface IAbstractRepository<T>
    where T : AbstractEntity
{
    IQueryable<T> GetAsQueryable();
    Task<T?> GetByIdAsync(long id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(long id);
    Task BatchDeleteAsync(Expression<Func<T, bool>> predicate);
}

public class AbstractRepository<T>(AppDbContext context) : IAbstractRepository<T>
    where T : AbstractEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public virtual IQueryable<T> GetAsQueryable()
    {
        return _dbSet.AsQueryable();
    }

    public virtual async Task<T?> GetByIdAsync(long id)
    {
        return await _dbSet.FindAsync(id);
    }

    public virtual async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public virtual async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public virtual async Task AddRangeAsync(IEnumerable<T> entity)
    {
        await _dbSet.AddRangeAsync(entity);
        await context.SaveChangesAsync();
    }

    public virtual async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await context.SaveChangesAsync();
    }

    public virtual async Task DeleteAsync(long id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            _dbSet.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    public virtual async Task BatchDeleteAsync(Expression<Func<T, bool>> predicate)
    {
        var entitiesToDelete = await _dbSet.Where(predicate).ToListAsync();

        if (entitiesToDelete.Count != 0)
        {
            _dbSet.RemoveRange(entitiesToDelete);
            await context.SaveChangesAsync();
        }
    }
}