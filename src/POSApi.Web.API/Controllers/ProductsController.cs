using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Products.Commands.ActivateProduct;
using POSApi.Application.Features.Products.Commands.ConvertUnits;
using POSApi.Application.Features.Products.Commands.CreateProduct;
using POSApi.Application.Features.Products.Commands.DeactivateProduct;
using POSApi.Application.Features.Products.Commands.UpdateProduct;
using POSApi.Application.Features.Products.Commands.UpdateProductPricing;
using POSApi.Application.Features.Products.Commands.UpdateStock;
using POSApi.Application.Features.Products.Queries.GetAllProducts;
using POSApi.Application.Features.Products.Queries.GetProductAnalytics;
using POSApi.Application.Features.Products.Queries.GetProductByBarcode;
using POSApi.Application.Features.Products.Queries.GetProductById;
using POSApi.Application.Features.Products.Queries.GetProductBySku;
using POSApi.Application.Features.Products.Queries.GetProductsByCategory;
using POSApi.Application.Features.Products.Queries.GetProductsByVendor;
using POSApi.Application.Features.Products.Queries.GetLowStockProducts;
using POSApi.Application.Features.Products.Queries.GetProductsNeedingReorder;
using POSApi.Application.Features.Products.Queries.GetUnitTypes;
using POSApi.Application.Features.Products.Queries.GetUnitsOfMeasure;
using POSApi.Application.Features.Products.Queries.SearchProducts;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with optional search, filtering, and pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Number of items per page (default: 20, max: 100)</param>
    /// <param name="searchTerm">Search term to filter by Name, SKU, Barcode, Category, or Description</param>
    /// <param name="categoryId">Filter by specific category ID</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="isLowStock">Filter to show only low stock products</param>
    /// <param name="includeInactive">Include inactive products in results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of products with search and filter capabilities</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAllProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isLowStock = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Limit maximum page size

        var query = new GetAllProductsQuery
        {
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            CategoryId = categoryId,
            IsActive = isActive,
            IsLowStock = isLowStock,
            IncludeInactive = includeInactive
        };

        var result = await _mediator.Send(query, cancellationToken);
        
        // Add pagination headers for easier frontend consumption
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Page", result.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());

        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProductDto>> GetProductById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetProductByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (result == null)
        {
            return NotFound($"Product with ID {id} not found");
        }
        
        return Ok(result);
    }

    /// <summary>
    /// Get product by SKU
    /// </summary>
    [HttpGet("sku/{sku}")]
    public async Task<ActionResult<ProductDto>> GetProductBySku(string sku, CancellationToken cancellationToken)
    {
        var query = new GetProductBySkuQuery(sku);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Product with SKU '{sku}' not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Get product by barcode
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetProductByBarcode(string barcode, CancellationToken cancellationToken)
    {
        var query = new GetProductByBarcodeQuery(barcode);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"Product with barcode '{barcode}' not found");
        }

        return Ok(result);
    }

    /// <summary>
    /// Search products by name, barcode, SKU, description, or category name (Legacy endpoint - consider using GET /products with search parameters)
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts(
        [FromQuery] string searchTerm,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] bool? isLowStock = null,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchProductsQuery(searchTerm, categoryId, isActive, isLowStock);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get products by category
    /// </summary>
    [HttpGet("category/{categoryId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetProductsByCategoryQuery
        {
            CategoryId = categoryId,
            IncludeInactive = includeInactive
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get low stock products
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetLowStockProducts(
        [FromQuery] bool includeOutOfStock = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLowStockProductsQuery
        {
            IncludeOutOfStock = includeOutOfStock
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get products needing reorder
    /// </summary>
    [HttpGet("reorder-needed")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsNeedingReorder(CancellationToken cancellationToken)
    {
        var query = new GetProductsNeedingReorderQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get products by vendor
    /// </summary>
    [HttpGet("vendor/{vendorId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByVendor(Guid vendorId, CancellationToken cancellationToken)
    {
        var query = new GetProductsByVendorQuery(vendorId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product with unit of measure support
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult<Guid>> CreateProduct([FromBody] CreateProductCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetProductById), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the request body");
        }

        try
        {
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Update product pricing (price and cost)
    /// </summary>
    [HttpPut("{id:guid}/pricing")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> UpdateProductPricing(Guid id, [FromBody] UpdateProductPricingRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateProductPricingCommand
            {
                ProductId = id,
                Price = request.Price,
                Cost = request.Cost
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
    /// Update product stock quantity
    /// </summary>
    [HttpPut("{id:guid}/stock")]
    public async Task<ActionResult> UpdateStock(Guid id, [FromBody] UpdateStockRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateStockCommand
            {
                ProductId = id,
                Quantity = request.NewQuantity
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
    /// Activate a product
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> ActivateProduct(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ActivateProductCommand
            {
                ProductId = id
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
    /// Deactivate a product
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Policy = "RequireManager")]
    public async Task<ActionResult> DeactivateProduct(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeactivateProductCommand
            {
                ProductId = id
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
    /// Get available units of measure
    /// </summary>
    [HttpGet("units-of-measure")]
    public async Task<ActionResult<IEnumerable<UnitOfMeasureDto>>> GetUnitsOfMeasure([FromQuery] string? unitTypeName = null, CancellationToken cancellationToken = default)
    {
        var query = new GetUnitsOfMeasureQuery
        {
            UnitTypeName = unitTypeName
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get unit types
    /// </summary>
    [HttpGet("unit-types")]
    public async Task<ActionResult<IEnumerable<UnitTypeDto>>> GetUnitTypes(CancellationToken cancellationToken)
    {
        var query = new GetUnitTypesQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Calculate unit conversions
    /// </summary>
    [HttpPost("convert-units")]
    public async Task<ActionResult<UnitConversionResultDto>> ConvertUnits([FromBody] UnitConversionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ConvertUnitsCommand
            {
                Quantity = request.Quantity,
                FromUnitCode = request.FromUnitCode,
                ToUnitCode = request.ToUnitCode
            };

            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Get product statistics and analytics
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<ProductAnalyticsDto>> GetProductAnalytics(CancellationToken cancellationToken)
    {
        var query = new GetProductAnalyticsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}
