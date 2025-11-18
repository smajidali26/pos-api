using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Inventory.Queries.GetInventorySummaryByLocation;

public class GetInventorySummaryByLocationQuery : IQuery<IEnumerable<LocationInventorySummaryDto>>
{
    public Guid? LocationId { get; set; }
    public bool IncludeInactive { get; set; } = false;
}

public class LocationInventorySummaryDto
{
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public int TotalProducts { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int OverStockCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
}
