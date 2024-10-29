using AspNetCore_Template.model.DTO.request.extendable;
using AspNetCore_Template.model.DTO.request.generic;
using AspNetCore_Template.model.DTO.response.extendable;
using AspNetCore_Template.model.DTO.response.generic;
using AspNetCore_Template.model.entity.abs;
using AspNetCore_Template.repository.abs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore_Template.service.abs;

public interface IAbstractService<TEntity, in TRequest, TResponse>
{
    Task<TResponse> GetByIdAsync(long id);
    Task<List<TResponse>> GetAllAsync();
    Task<List<SelectOptionResponse>> GetAllAsOptionsAsync();
    Task<TResponse> InsertAsync(TRequest request);
    Task<TResponse> UpdateAsync(long id, TRequest request);
    Task DeleteAsync(long id);
    Task BatchDeleteAsync(IEnumerable<IdRequest> requestList);
    Task<List<T>> ProjectFromQueryToListAsync<T>(IQueryable<AbstractEntity> query) where T : class, IResponse;
}

public abstract class AbstractService<TEntity, TRequest, TResponse, TRepository>(
    TRepository repository,
    IMapper mapper
) : IAbstractService<TEntity, TRequest, TResponse>
    where TEntity : AbstractEntity
    where TRequest : class, IRequest
    where TResponse : class, IResponse
    where TRepository : IAbstractRepository<TEntity>
{
    private TRepository _repository = repository;


    public async Task<TResponse> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Entity with id {id} not found.");
        return mapper.Map<TResponse>(entity);
    }

    public virtual async Task<List<TResponse>> GetAllAsync()
    {
        return await ProjectFromQueryToListAsync<TResponse>(_repository.GetAsQueryable());
    }

    public virtual async Task<List<SelectOptionResponse>> GetAllAsOptionsAsync()
    {
        return await ProjectFromQueryToListAsync<SelectOptionResponse>(_repository.GetAsQueryable());
    }

    public async Task<TResponse> InsertAsync(TRequest request)
    {
        var entity = mapper.Map<TEntity>(request);
        await _repository.AddAsync(entity);
        return mapper.Map<TResponse>(entity);
    }

    public async Task<TResponse> UpdateAsync(long id, TRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) throw new KeyNotFoundException($"Entity with id {id} not found.");
        mapper.Map(request, entity);
        await _repository.UpdateAsync(entity);
        return mapper.Map<TResponse>(entity);
    }

    public async Task DeleteAsync(long id)
    {
        await _repository.DeleteAsync(id);
    }

    public async Task BatchDeleteAsync(IEnumerable<IdRequest> requestList)
    {
        var ids = requestList.Select(req => req.Id);
        await _repository.BatchDeleteAsync(i => ids.Contains(i.Id));
    }

    public async Task<List<TR>> ProjectFromQueryToListAsync<TR>(IQueryable<AbstractEntity> query)
        where TR : class, IResponse
    {
        return await query.ProjectTo<TR>(mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<IEnumerable<TResponse>> InsertRangeAsync(IEnumerable<TRequest> request)
    {
        var entities = mapper.Map<List<TEntity>>(request);
        await _repository.AddRangeAsync(entities);
        return mapper.Map<IEnumerable<TResponse>>(entities);
    }
}