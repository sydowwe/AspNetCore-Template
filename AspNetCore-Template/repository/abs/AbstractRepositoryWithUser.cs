using System.Linq.Expressions;
using AspNetCore_Template.model.entity.abs;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore_Template.repository.abs;

public interface IAbstractRepositoryWithUser<T> : IAbstractRepository<T>
    where T : AbstractEntity
{
    IQueryable<T> GetAsQueryable(long userId);
    Task<List<T>> GetAllAsync(long userId);
}

public class AbstractRepositoryWithUser<T>(AppDbContext context) : AbstractRepository<T>(context), IAbstractRepositoryWithUser<T>
    where T : AbstractEntity
{
    private readonly DbSet<T> _dbSet = context.Set<T>();

    public IQueryable<T> GetAsQueryable(long userId)
    {
        return _dbSet.AsQueryable();
    }
    public async Task<List<T>> GetAllAsync(long userId)
    {
        return await _dbSet.ToListAsync();
    }
}