using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Features.Stores.Commands.ActivateStore;
using POSApi.Application.Features.Stores.Commands.CreateStore;
using POSApi.Application.Features.Stores.Commands.DeactivateStore;
using POSApi.Application.Features.Stores.Commands.UpdateStore;
using POSApi.Application.Features.Stores.Commands.UpdateStoreManager;
using POSApi.Application.Features.Stores.Queries.GetAllStores;
using POSApi.Application.Features.Stores.Queries.GetStoreById;
using POSApi.Application.Features.Stores.Queries.GetStoreHierarchy;
using POSApi.Application.Features.Stores.Queries.GetStoreInventory;
using POSApi.Application.Features.Stores.Queries.GetStoreSummary;
using POSApi.Application.Features.StoreInventory.Commands.AdjustStoreInventory;
using POSApi.Application.Features.StoreInventory.Commands.UpdateStockLevels;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoresController : ControllerBase
{
    private readonly IMediator _mediator;

    public StoresController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all stores with optional filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<StoreDto>>> GetAllStores(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? parentStoreId = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var query = new GetAllStoresQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsActive = isActive,
            ParentStoreId = parentStoreId
        };

        var result = await _mediator.Send(query, cancellationToken);

        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Page", result.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(result);
    }

    /// <summary>
    /// Get store by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StoreDto>> GetStoreById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetStoreByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Store with ID {id} not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Get store hierarchy tree
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<ActionResult<List<StoreHierarchyDto>>> GetStoreHierarchy(
        [FromQuery] Guid? rootStoreId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreHierarchyQuery { RootStoreId = rootStoreId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get store inventory
    /// </summary>
    [HttpGet("{id:guid}/inventory")]
    public async Task<ActionResult<List<StoreInventoryDto>>> GetStoreInventory(
        Guid id,
        [FromQuery] bool? lowStockOnly = null,
        [FromQuery] bool? outOfStockOnly = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStoreInventoryQuery
        {
            StoreId = id,
            LowStockOnly = lowStockOnly,
            OutOfStockOnly = outOfStockOnly
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get store summary dashboard
    /// </summary>
    [HttpGet("{id:guid}/summary")]
    public async Task<ActionResult<StoreSummaryDto>> GetStoreSummary(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetStoreSummaryQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Store with ID {id} not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new store
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateStore(
        [FromBody] CreateStoreRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateStoreCommand
            {
                Name = request.Name,
                Code = request.Code,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                ManagerUserId = request.ManagerUserId,
                StoreType = request.StoreType,
                ParentStoreId = request.ParentStoreId,
                Notes = request.Notes,
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,
                TimeZone = request.TimeZone,
                TaxRate = request.TaxRate,
                Currency = request.Currency
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetStoreById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing store
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateStore(
        Guid id,
        [FromBody] UpdateStoreRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateStoreCommand
            {
                Id = id,
                Name = request.Name,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Country = request.Country,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                Notes = request.Notes,
                StoreType = request.StoreType,
                ParentStoreId = request.ParentStoreId,
                OpenTime = request.OpenTime,
                CloseTime = request.CloseTime,
                TimeZone = request.TimeZone,
                TaxRate = request.TaxRate,
                Currency = request.Currency
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Activate a store
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ActivateStore(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ActivateStoreCommand { StoreId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Deactivate a store
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DeactivateStore(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeactivateStoreCommand { StoreId = id };
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update store manager
    /// </summary>
    [HttpPost("{id:guid}/manager")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateStoreManager(
        Guid id,
        [FromBody] UpdateStoreManagerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateStoreManagerCommand
            {
                StoreId = id,
                ManagerUserId = request.ManagerUserId
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Adjust store inventory
    /// </summary>
    [HttpPost("{id:guid}/inventory/adjust")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> AdjustStoreInventory(
        Guid id,
        [FromBody] AdjustStoreInventoryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AdjustStoreInventoryCommand
            {
                StoreId = id,
                ProductId = request.ProductId,
                AdjustmentAmount = request.AdjustmentAmount,
                Reason = request.Reason
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update store inventory stock levels
    /// </summary>
    [HttpPost("{id:guid}/inventory/stock-levels")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateStockLevels(
        Guid id,
        [FromBody] UpdateStockLevelsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateStockLevelsCommand
            {
                StoreId = id,
                ProductId = request.ProductId,
                MinStockLevel = request.MinStockLevel,
                MaxStockLevel = request.MaxStockLevel,
                ReorderPoint = request.ReorderPoint
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
