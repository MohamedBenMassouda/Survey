using Microsoft.AspNetCore.Mvc;
using Survey.Infrastructure.DTO;
using Survey.Infrastructure.Interfaces;
using Survey.Infrastructure.Models;

namespace Survey.Api.Controllers;

[ApiController]
[Route("api/[controller]s")]
public class BaseController<TEntity>(IUnitOfWork unitOfWork) : ControllerBase where TEntity : BaseEntity
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IRepository<TEntity> _repository = unitOfWork.Repository<TEntity>();

    [HttpGet]
    public async Task<ActionResult> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await _repository.GetPagedAsync(pagination.PageNumber, pagination.PageSize);

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> GetById(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);

        if (entity != null)
        {
            return Ok(entity);
        }

        var entityName = typeof(TEntity).Name;

        return NotFound($"{entityName} with ID {id} not found.");

    }
}