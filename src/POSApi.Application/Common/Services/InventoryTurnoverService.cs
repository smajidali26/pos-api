using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Common.Services;

public class InventoryTurnoverService : IInventoryTurnoverService
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryTurnoverService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<InventoryTurnoverSummaryDto> CalculateTurnoverRatio(
        Guid? storeId = null, Guid? categoryId = null, int periodDays = 365, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-periodDays);

        // Get all products with their current inventory
        var productsQuery = _unitOfWork.Context.Set<Product>()
            .Include(p => p.Category)
            .Where(p => p.IsActive);

        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
        }

        var products = await productsQuery.ToListAsync(cancellationToken);

        var turnoverItems = new List<InventoryTurnoverDto>();

        foreach (var product in products)
        {
            // Calculate COGS (Cost of Goods Sold) for the period
            var salesQuery = _unitOfWork.Context.Set<Order>()
                .Where(o => o.OrderDate >= startDate && o.Status != OrderStatus.Cancelled)
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.ProductId == product.Id);

            if (storeId.HasValue)
            {
                salesQuery = salesQuery.Where(oi => oi.Order.StoreId == storeId.Value);
            }

            var sales = await salesQuery
                .Select(oi => new { oi.Quantity, Cost = oi.Product.Cost })
                .ToListAsync(cancellationToken);

            var totalQuantitySold = sales.Sum(s => s.Quantity);
            var cogs = sales.Sum(s => s.Quantity * s.Cost);

            // Average inventory value (simplified: current stock * cost)
            var averageInventoryValue = product.StockQuantity * product.Cost;

            // Calculate turnover ratio: COGS / Average Inventory Value
            var turnoverRatio = averageInventoryValue > 0 ? cogs / averageInventoryValue : 0;

            // Annualize the turnover ratio
            var annualizedTurnoverRatio = turnoverRatio * (365m / periodDays);

            // Calculate days to sell
            var daysToSell = annualizedTurnoverRatio > 0 ? 365 / annualizedTurnoverRatio : 0;

            var classification = ClassifyTurnoverRate(annualizedTurnoverRatio);

            // Estimate months of supply
            var averageDailySales = totalQuantitySold / (decimal)periodDays;
            var monthsOfSupply = averageDailySales > 0 ? (product.StockQuantity / averageDailySales) / 30 : 999;

            var recommendation = GenerateRecommendation(classification, product.StockQuantity,
                averageDailySales, monthsOfSupply, product.ReorderLevel);

            turnoverItems.Add(new InventoryTurnoverDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                SKU = product.SKU,
                CategoryName = product.Category?.Name ?? "Unknown",
                StoreId = storeId,
                TurnoverRatio = Math.Round(annualizedTurnoverRatio, 2),
                DaysToSell = Math.Round(daysToSell, 2),
                AverageCOGS = Math.Round(cogs, 2),
                AverageInventoryValue = Math.Round(averageInventoryValue, 2),
                Classification = classification,
                CurrentStock = product.StockQuantity,
                EstimatedMonthsOfSupply = Math.Round(Math.Min(monthsOfSupply, 999), 2),
                Recommendation = recommendation
            });
        }

        // Calculate category-wise turnover
        var categoryTurnover = turnoverItems
            .GroupBy(t => new { CategoryName = t.CategoryName })
            .Select(g => new CategoryTurnoverDto
            {
                CategoryName = g.Key.CategoryName,
                AverageTurnoverRatio = g.Average(t => t.TurnoverRatio),
                TotalInventoryValue = g.Sum(t => t.AverageInventoryValue),
                ProductCount = g.Count()
            })
            .OrderByDescending(c => c.TotalInventoryValue)
            .ToList();

        return new InventoryTurnoverSummaryDto
        {
            OverallTurnoverRatio = turnoverItems.Any() ? Math.Round(turnoverItems.Average(t => t.TurnoverRatio), 2) : 0,
            AverageDaysToSell = turnoverItems.Any() ? Math.Round(turnoverItems.Average(t => t.DaysToSell), 2) : 0,
            FastMovingItems = turnoverItems.Count(t => t.Classification == "Fast"),
            NormalMovingItems = turnoverItems.Count(t => t.Classification == "Normal"),
            SlowMovingItems = turnoverItems.Count(t => t.Classification == "Slow"),
            DeadStockItems = turnoverItems.Count(t => t.Classification == "Dead"),
            TotalInventoryValue = Math.Round(turnoverItems.Sum(t => t.AverageInventoryValue), 2),
            Items = turnoverItems.OrderBy(t => t.TurnoverRatio).ToList(),
            ByCategory = categoryTurnover
        };
    }

    public async Task<List<InventoryTurnoverDto>> IdentifySlowMovingStock(
        Guid? storeId = null, CancellationToken cancellationToken = default)
    {
        var turnoverData = await CalculateTurnoverRatio(storeId, null, 365, cancellationToken);

        return turnoverData.Items
            .Where(t => t.Classification == "Slow" || t.Classification == "Dead")
            .OrderBy(t => t.TurnoverRatio)
            .ToList();
    }

    public async Task<List<ReorderRecommendationDto>> GenerateReorderRecommendations(
        Guid? storeId = null, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-90);

        var productsQuery = _unitOfWork.Context.Set<Product>()
            .Include(p => p.PrimaryVendor)
            .Where(p => p.IsActive);

        var products = await productsQuery.ToListAsync(cancellationToken);
        var recommendations = new List<ReorderRecommendationDto>();

        foreach (var product in products)
        {
            // Calculate average daily sales
            var salesQuery = _unitOfWork.Context.Set<Order>()
                .Where(o => o.OrderDate >= startDate && o.Status != OrderStatus.Cancelled)
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.ProductId == product.Id);

            if (storeId.HasValue)
            {
                salesQuery = salesQuery.Where(oi => oi.Order.StoreId == storeId.Value);
            }

            var totalSold = await salesQuery.SumAsync(oi => oi.Quantity, cancellationToken);
            var averageDailySales = totalSold / 90m;

            // Check if reorder is needed
            if (product.StockQuantity <= product.ReorderLevel && product.ReorderQuantity > 0)
            {
                var daysUntilStockout = averageDailySales > 0
                    ? (int)Math.Ceiling(product.StockQuantity / averageDailySales)
                    : 999;

                // Calculate recommended order quantity (Economic Order Quantity simplified)
                var leadTimeDemand = averageDailySales * 7; // Assume 7 day lead time
                var safetyStock = averageDailySales * 14; // 2 weeks safety stock
                var recommendedQty = (int)Math.Ceiling(leadTimeDemand + safetyStock - product.StockQuantity);

                // Use configured reorder quantity if higher
                recommendedQty = Math.Max(recommendedQty, product.ReorderQuantity);

                var priority = daysUntilStockout switch
                {
                    <= 3 => "Critical",
                    <= 7 => "High",
                    <= 14 => "Medium",
                    _ => "Low"
                };

                recommendations.Add(new ReorderRecommendationDto
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    SKU = product.SKU,
                    CurrentStock = product.StockQuantity,
                    ReorderLevel = product.ReorderLevel,
                    RecommendedOrderQuantity = recommendedQty,
                    AverageDailySales = Math.Round(averageDailySales, 2),
                    DaysUntilStockout = daysUntilStockout,
                    Priority = priority,
                    PreferredVendorId = product.PrimaryVendorId,
                    VendorName = product.PrimaryVendor?.Name
                });
            }
        }

        return recommendations
            .OrderBy(r => r.Priority switch { "Critical" => 1, "High" => 2, "Medium" => 3, _ => 4 })
            .ThenBy(r => r.DaysUntilStockout)
            .ToList();
    }

    public string ClassifyTurnoverRate(decimal turnoverRatio)
    {
        // Classification based on industry standards
        // Fast: > 12 times per year (monthly)
        // Normal: 4-12 times per year (quarterly to monthly)
        // Slow: 1-4 times per year (annually to quarterly)
        // Dead: < 1 time per year

        return turnoverRatio switch
        {
            > 12 => "Fast",
            >= 4 => "Normal",
            >= 1 => "Slow",
            _ => "Dead"
        };
    }

    private string GenerateRecommendation(string classification, int currentStock,
        decimal averageDailySales, decimal monthsOfSupply, int reorderLevel)
    {
        return classification switch
        {
            "Fast" => currentStock <= reorderLevel
                ? "URGENT: Reorder immediately. High turnover product at or below reorder level."
                : $"Monitor closely. High demand item with {monthsOfSupply:F1} months supply.",

            "Normal" => currentStock <= reorderLevel
                ? "Consider reordering. Stock approaching minimum levels."
                : $"Adequate inventory. {monthsOfSupply:F1} months supply available.",

            "Slow" => monthsOfSupply > 6
                ? "OVERSTOCKED: Reduce ordering. Consider promotions to move inventory."
                : "Monitor demand patterns. Adjust reorder quantities if needed.",

            "Dead" => "DEAD STOCK: No recent sales. Consider clearance, returns to vendor, or discontinuation.",

            _ => "Insufficient data for recommendation."
        };
    }
}
