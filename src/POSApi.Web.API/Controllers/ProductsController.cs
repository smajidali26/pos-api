using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Products.Commands.CreateProduct;
using POSApi.Application.Features.Products.Commands.UpdateProduct;
using POSApi.Application.Features.Products.Commands.UpdateStock;
using POSApi.Application.Features.Products.Queries.GetAllProducts;
using POSApi.Application.Features.Products.Queries.GetProductById;
using POSApi.Application.Features.Products.Queries.SearchProducts;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IMediator mediator, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _unitOfWork = unitOfWork;
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
        var product = await _unitOfWork.Products.GetBySkuAsync(sku, cancellationToken);
        
        if (product == null)
        {
            return NotFound($"Product with SKU '{sku}' not found");
        }

        // Simple mapping without AutoMapper for now
        var result = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Barcode = product.Barcode,
            Price = product.Price,
            Cost = product.Cost,
            StockQuantity = product.StockQuantity,
            MinStockLevel = product.MinStockLevel,
            ReorderLevel = product.ReorderLevel,
            ReorderQuantity = product.ReorderQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
        
        return Ok(result);
    }

    /// <summary>
    /// Get product by barcode
    /// </summary>
    [HttpGet("barcode/{barcode}")]
    public async Task<ActionResult<ProductDto>> GetProductByBarcode(string barcode, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByBarcodeAsync(barcode, cancellationToken);
        
        if (product == null)
        {
            return NotFound($"Product with barcode '{barcode}' not found");
        }

        // Simple mapping without AutoMapper for now
        var result = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            SKU = product.SKU,
            Barcode = product.Barcode,
            Price = product.Price,
            Cost = product.Cost,
            StockQuantity = product.StockQuantity,
            MinStockLevel = product.MinStockLevel,
            ReorderLevel = product.ReorderLevel,
            ReorderQuantity = product.ReorderQuantity,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
        
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
        var products = await _unitOfWork.Products.GetByCategoryIdAsync(categoryId, cancellationToken);

        if (!includeInactive)
        {
            products = products.Where(p => p.IsActive);
        }

        var result = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            SKU = p.SKU,
            Barcode = p.Barcode,
            Price = p.Price,
            Cost = p.Cost,
            StockQuantity = p.StockQuantity,
            MinStockLevel = p.MinStockLevel,
            ReorderLevel = p.ReorderLevel,
            ReorderQuantity = p.ReorderQuantity,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        });

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
        var products = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);

        if (!includeOutOfStock)
        {
            products = products.Where(p => p.StockQuantity > 0);
        }

        var result = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            SKU = p.SKU,
            Barcode = p.Barcode,
            Price = p.Price,
            Cost = p.Cost,
            StockQuantity = p.StockQuantity,
            MinStockLevel = p.MinStockLevel,
            ReorderLevel = p.ReorderLevel,
            ReorderQuantity = p.ReorderQuantity,
            IsActive = p.IsActive,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name ?? string.Empty,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            IsLowStock = p.IsLowStock,
            IsOutOfStock = p.IsOutOfStock,
            NeedsReorder = p.NeedsReorder
        }).OrderBy(p => p.StockQuantity);

        return Ok(result);
    }

    /// <summary>
    /// Get products needing reorder
    /// </summary>
    [HttpGet("reorder-needed")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsNeedingReorder(CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsNeedingReorderAsync(cancellationToken);
        
        var result = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            StockQuantity = p.StockQuantity,
            MinStockLevel = p.MinStockLevel,
            ReorderLevel = p.ReorderLevel,
            ReorderQuantity = p.ReorderQuantity,
            CategoryName = p.Category?.Name ?? string.Empty,
            IsActive = p.IsActive,
            NeedsReorder = p.NeedsReorder
        });

        return Ok(result);
    }

    /// <summary>
    /// Get products by vendor
    /// </summary>
    [HttpGet("vendor/{vendorId:guid}")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByVendor(Guid vendorId, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsByVendorAsync(vendorId, cancellationToken);
        
        var result = products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            SKU = p.SKU,
            Barcode = p.Barcode,
            Price = p.Price,
            Cost = p.Cost,
            StockQuantity = p.StockQuantity,
            CategoryName = p.Category?.Name ?? string.Empty,
            IsActive = p.IsActive
        });

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
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            product.UpdatePrice(request.Price);
            product.UpdateCost(request.Cost);

            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            product.Activate();
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
            var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
            if (product == null)
            {
                return NotFound($"Product with ID {id} not found");
            }

            product.Deactivate();
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        var units = string.IsNullOrEmpty(unitTypeName) 
            ? await _unitOfWork.UnitsOfMeasure.GetActiveAsync(cancellationToken)
            : await _unitOfWork.UnitsOfMeasure.GetByUnitTypeNameAsync(unitTypeName, cancellationToken);

        var result = units.Select(u => new UnitOfMeasureDto
        {
            Code = u.Code,
            Name = u.Name,
            Symbol = u.Symbol,
            Type = u.UnitType?.Name ?? string.Empty,
            ConversionFactorToBase = u.ConversionFactorToBase,
            IsBaseUnit = u.IsBaseUnit
        });

        return Ok(result);
    }

    /// <summary>
    /// Get unit types
    /// </summary>
    [HttpGet("unit-types")]
    public async Task<ActionResult<IEnumerable<object>>> GetUnitTypes(CancellationToken cancellationToken)
    {
        var unitTypes = await _unitOfWork.UnitTypes.GetActiveAsync(cancellationToken);
        
        var result = unitTypes.Select(ut => new 
        { 
            Id = ut.Id,
            Name = ut.Name,
            Description = ut.Description,
            SortOrder = ut.SortOrder
        });
        
        return Ok(result);
    }

    /// <summary>
    /// Calculate unit conversions
    /// </summary>
    [HttpPost("convert-units")]
    public async Task<ActionResult<object>> ConvertUnits([FromBody] UnitConversionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var fromUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.FromUnitCode, cancellationToken);
            var toUnit = await _unitOfWork.UnitsOfMeasure.GetByCodeAsync(request.ToUnitCode, cancellationToken);

            if (fromUnit == null)
            {
                return BadRequest($"Invalid from unit code: {request.FromUnitCode}");
            }

            if (toUnit == null)
            {
                return BadRequest($"Invalid to unit code: {request.ToUnitCode}");
            }

            var convertedQuantity = fromUnit.ConvertTo(request.Quantity, toUnit);

            return Ok(new
            {
                FromQuantity = request.Quantity,
                FromUnit = fromUnit.Symbol,
                ToQuantity = convertedQuantity,
                ToUnit = toUnit.Symbol,
                ConversionFactor = convertedQuantity / request.Quantity
            });
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
    public async Task<ActionResult<object>> GetProductAnalytics(CancellationToken cancellationToken)
    {
        var allProducts = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var lowStockProducts = await _unitOfWork.Products.GetLowStockProductsAsync(cancellationToken);
        var productsNeedingReorder = await _unitOfWork.Products.GetProductsNeedingReorderAsync(cancellationToken);

        var analytics = new
        {
            TotalProducts = allProducts.Count(),
            ActiveProducts = allProducts.Count(p => p.IsActive),
            InactiveProducts = allProducts.Count(p => !p.IsActive),
            LowStockProducts = lowStockProducts.Count(),
            OutOfStockProducts = allProducts.Count(p => p.StockQuantity <= 0),
            ProductsNeedingReorder = productsNeedingReorder.Count(),
            TotalInventoryValue = allProducts.Where(p => p.IsActive).Sum(p => p.GetInventoryValue()),
            AverageProductPrice = allProducts.Where(p => p.IsActive && p.Price > 0).Average(p => p.Price),
            Categories = allProducts.Where(p => p.Category != null)
                .GroupBy(p => p.Category!.Name)
                .Select(g => new { CategoryName = g.Key, ProductCount = g.Count() })
                .OrderByDescending(x => x.ProductCount)
                .Take(10)
        };

        return Ok(analytics);
    }
}
