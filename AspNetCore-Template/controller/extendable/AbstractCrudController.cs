using AspNetCore_Template.model.DTO.request.extendable;
using AspNetCore_Template.model.DTO.request.generic;
using AspNetCore_Template.model.DTO.response.extendable;
using AspNetCore_Template.model.DTO.response.generic;
using AspNetCore_Template.model.entity.abs;
using AspNetCore_Template.service.abs;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore_Template.controller.extendable;

public abstract class AbstractCrudController<TEntity, TRequest, TResponse, TService>(TService service) : ControllerBase
    where TEntity : AbstractEntity
    where TRequest : IRequest
    where TResponse : IIdResponse
    where TService : IAbstractService<TEntity, TRequest, TResponse>
{
    private TService _service = service;

    [HttpPost("get-all")]
    public virtual async Task<ActionResult<IEnumerable<TResponse>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpPost("get-all-options")]
    public virtual async Task<ActionResult<IEnumerable<SelectOptionResponse>>> GetAllOptions()
    {
        return Ok(await _service.GetAllAsOptionsAsync());
    }

    [HttpGet("{id:long}")]
    public virtual async Task<ActionResult<TResponse>> Get(long id)
    {
        var response = await _service.GetByIdAsync(id);
        if (response == null)
            return NotFound();
        return Ok(response);
    }

    [HttpPost("create")]
    public virtual async Task<ActionResult<TResponse>> Create(TRequest request)
    {
        var newItem = await _service.InsertAsync(request);
        return CreatedAtAction(nameof(Get), new { id = newItem.Id }, newItem);
    }

    [HttpPut("{id:long}")]
    public virtual async Task<ActionResult<TResponse>> Update(long id, TRequest request)
    {
        var updatedItem = await _service.UpdateAsync(id, request);
        if (updatedItem == null)
            return NotFound();
        return Ok(updatedItem);
    }

    [HttpDelete("{id:long}")]
    public virtual async Task<ActionResult<IdResponse>> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(id);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return NotFound();
        }

        return Ok(new IdResponse(id));
    }

    [HttpPost("batch-delete")]
    public virtual async Task<ActionResult<SuccessResponse>> BatchDelete(List<IdRequest> request)
    {
        await _service.BatchDeleteAsync(request);
        return Ok(new SuccessResponse("deleted"));
    }
}