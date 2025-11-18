using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Inventory.Queries.GetInventorySummaryByLocation;

public class GetInventorySummaryByLocationQueryHandler : IQueryHandler<GetInventorySummaryByLocationQuery, IEnumerable<LocationInventorySummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInventorySummaryByLocationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<LocationInventorySummaryDto>> Handle(GetInventorySummaryByLocationQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.Locations.AsQueryable();

        if (request.LocationId.HasValue)
            query = query.Where(l => l.Id == request.LocationId.Value);

        if (!request.IncludeInactive)
            query = query.Where(l => l.IsActive);

        var locations = await query.ToListAsync(cancellationToken);
        var summaries = new List<LocationInventorySummaryDto>();

        foreach (var location in locations)
        {
            var productLocations = await _unitOfWork.Context.ProductLocations
                .Include(pl => pl.Product)
                .Where(pl => pl.LocationId == location.Id)
                .ToListAsync(cancellationToken);

            var summary = new LocationInventorySummaryDto
            {
                LocationId = location.Id,
                LocationName = location.Name,
                LocationCode = location.Code,
                TotalProducts = productLocations.Count,
                LowStockCount = productLocations.Count(pl => pl.IsLowStock),
                OutOfStockCount = productLocations.Count(pl => pl.IsOutOfStock),
                OverStockCount = productLocations.Count(pl => pl.IsOverStock),
                TotalInventoryValue = productLocations.Sum(pl => pl.Quantity * pl.Product.Price)
            };

            summaries.Add(summary);
        }

        return summaries;
    }
}
