using POSApi.Application.Common.DTOs.Reports;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Reports.Queries.GetInventoryReport;

public class GetInventoryReportQuery : IQuery<InventoryReportDto>
{
    public DateTime? AsOfDate { get; set; }

    public GetInventoryReportQuery(DateTime? asOfDate = null)
    {
        AsOfDate = asOfDate ?? DateTime.Now.Date;
    }
}

public class GetInventoryReportQueryHandler : IQueryHandler<GetInventoryReportQuery, InventoryReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetInventoryReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InventoryReportDto> Handle(GetInventoryReportQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetAllAsync(cancellationToken);
        var productList = products.ToList();

        var report = new InventoryReportDto
        {
            ReportDate = request.AsOfDate ?? DateTime.Now.Date,
            TotalProducts = productList.Count,
            LowStockProducts = productList.Count(p => p.StockQuantity <= p.MinStockLevel && p.StockQuantity > 0),
            OutOfStockProducts = productList.Count(p => p.StockQuantity == 0),
            TotalInventoryValue = productList.Sum(p => p.StockQuantity * p.Cost)
        };

        // Product inventory details
        report.ProductInventory = productList
            .Select(p => new ProductInventoryDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                SKU = p.SKU,
                CategoryName = p.Category?.Name ?? "Uncategorized",
                CurrentStock = p.StockQuantity,
                MinStockLevel = p.MinStockLevel,
                UnitCost = p.Cost,
                UnitPrice = p.Price,
                TotalValue = p.StockQuantity * p.Cost,
                StockStatus = GetStockStatus(p.StockQuantity, p.MinStockLevel)
            })
            .OrderBy(p => p.CategoryName)
            .ThenBy(p => p.ProductName)
            .ToList();

        // Category breakdown
        report.CategoryBreakdown = productList
            .GroupBy(p => new { 
                CategoryId = p.CategoryId, 
                CategoryName = p.Category?.Name ?? "Uncategorized" 
            })
            .Select(g => new CategoryInventoryDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                ProductCount = g.Count(),
                TotalStock = g.Sum(p => p.StockQuantity),
                TotalValue = g.Sum(p => p.StockQuantity * p.Cost)
            })
            .OrderByDescending(c => c.TotalValue)
            .ToList();

        return report;
    }

    private static string GetStockStatus(int currentStock, int minStockLevel)
    {
        if (currentStock == 0)
            return "Out of Stock";
        if (currentStock <= minStockLevel)
            return "Low Stock";
        return "Normal";
    }
}