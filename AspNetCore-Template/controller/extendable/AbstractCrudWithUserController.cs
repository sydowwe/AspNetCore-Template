using AspNetCore_Template.model.DTO.request.extendable;
using AspNetCore_Template.model.DTO.request.generic;
using AspNetCore_Template.model.DTO.response.extendable;
using AspNetCore_Template.model.DTO.response.generic;
using AspNetCore_Template.model.entity.abs;
using AspNetCore_Template.repository.abs;
using AspNetCore_Template.service.abs;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore_Template.controller.extendable;

public abstract class AbstractCrudWithUserController<TEntity, TRequest, TResponse, TService>(TService service) : AbstractCrudController<TEntity, TRequest, TResponse, TService>(service)
    where TEntity : AbstractEntity
    where TRequest : IRequest
    where TResponse : IIdResponse
    where TService : IAbstractServiceWithUser<TEntity, TRequest, TResponse>
{
    private TService _service = service;

    [HttpPost("get-all")]
    public override async Task<ActionResult<IEnumerable<TResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost("get-all-options")]
    public override async Task<ActionResult<IEnumerable<SelectOptionResponse>>> GetAllOptions()
    {
        return Ok(await _service.GetAllAsOptionsAsync());
    }
}