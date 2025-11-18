using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Inventory.Queries.GetStockLevel;

public class GetStockLevelQueryHandler : IQueryHandler<GetStockLevelQuery, ProductLocationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStockLevelQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ProductLocationDto> Handle(GetStockLevelQuery request, CancellationToken cancellationToken)
    {
        if (!request.LocationId.HasValue)
        {
            // Return total stock across all locations
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

            return new ProductLocationDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSKU = product.SKU,
                Quantity = product.StockQuantity,
                MinStockLevel = product.MinStockLevel,
                ReorderPoint = product.ReorderLevel,
                IsLowStock = product.IsLowStock,
                IsOutOfStock = product.IsOutOfStock
            };
        }

        // Return stock at specific location
        var productLocation = await _unitOfWork.Context.ProductLocations
            .Include(pl => pl.Product)
            .Include(pl => pl.Location)
            .FirstOrDefaultAsync(pl => pl.ProductId == request.ProductId && pl.LocationId == request.LocationId.Value, cancellationToken);

        if (productLocation == null)
            throw new InvalidOperationException($"Product not found at location");

        return _mapper.Map<ProductLocationDto>(productLocation);
    }
}
