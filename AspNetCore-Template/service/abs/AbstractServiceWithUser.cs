using AspNetCore_Template.model.DTO.request.extendable;
using AspNetCore_Template.model.DTO.response.extendable;
using AspNetCore_Template.model.DTO.response.generic;
using AspNetCore_Template.model.entity.abs;
using AspNetCore_Template.repository.abs;
using AspNetCore_Template.security;
using AutoMapper;

namespace AspNetCore_Template.service.abs;

public interface IAbstractServiceWithUser<TEntity, in TRequest, TResponse> : IAbstractService<TEntity, TRequest, TResponse>
{
}

public abstract class AbstractServiceWithUser<TEntity, TRequest, TResponse, TRepository>(
    TRepository repository,
    ILoggedUserService loggedUserService,
    IMapper mapper
) : AbstractService<TEntity, TRequest, TResponse, TRepository>(repository, mapper), IAbstractServiceWithUser<TEntity, TRequest, TResponse>
    where TEntity : AbstractEntityWithUser
    where TRequest : class, IRequest
    where TResponse : class, IResponse
    where TRepository : IAbstractRepositoryWithUser<TEntity>
{
    private TRepository _repository = repository;
    private readonly IMapper _mapper = mapper;

    public override async Task<List<TResponse>> GetAllAsync()
    {
        return await ProjectFromQueryToListAsync<TResponse>(_repository.GetAsQueryable(loggedUserService.GetLoggedUserId()));
    }

    public override async Task<List<SelectOptionResponse>> GetAllAsOptionsAsync()
    {
        return await ProjectFromQueryToListAsync<SelectOptionResponse>(_repository.GetAsQueryable(loggedUserService.GetLoggedUserId()));
    }
    protected long GetLoggedUserId()
    {
        return loggedUserService.GetLoggedUserId();
    }
}