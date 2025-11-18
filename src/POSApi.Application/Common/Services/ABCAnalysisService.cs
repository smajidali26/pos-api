using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Common.Services;

public class ABCAnalysisService : IABCAnalysisService
{
    private readonly IUnitOfWork _unitOfWork;

    public ABCAnalysisService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ABCAnalysisDto> CalculateABCClassification(
        Guid? storeId = null, int periodDays = 365, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.UtcNow.AddDays(-periodDays);

        // Get sales data for all products
        var query = _unitOfWork.Context.Set<Order>()
            .Where(o => o.OrderDate >= startDate && o.Status != OrderStatus.Cancelled);

        if (storeId.HasValue)
        {
            query = query.Where(o => o.StoreId == storeId.Value);
        }

        var productSales = await query
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new { oi.ProductId, ProductName = oi.Product.Name, oi.Product.SKU, CategoryName = oi.Product.Category.Name })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.SKU,
                g.Key.CategoryName,
                Volume = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderByDescending(p => p.Revenue)
            .ToListAsync(cancellationToken);

        if (!productSales.Any())
        {
            return new ABCAnalysisDto
            {
                Summary = new ABCAnalysisSummaryDto(),
                CalculatedAt = DateTime.UtcNow
            };
        }

        var totalRevenue = productSales.Sum(p => p.Revenue);
        var totalProducts = productSales.Count;

        // Calculate cumulative percentages and classify
        var classifiedProducts = new List<ABCClassificationItemDto>();
        var cumulativeRevenue = 0m;
        var rank = 1;

        foreach (var product in productSales)
        {
            cumulativeRevenue += product.Revenue;
            var contributionPercentage = totalRevenue > 0 ? (product.Revenue / totalRevenue) * 100 : 0;
            var cumulativePercentage = totalRevenue > 0 ? (cumulativeRevenue / totalRevenue) * 100 : 0;

            var classification = ClassifyProduct(contributionPercentage, cumulativePercentage);

            classifiedProducts.Add(new ABCClassificationItemDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                SKU = product.SKU,
                CategoryName = product.CategoryName,
                StoreId = storeId,
                Classification = classification,
                AnnualVolume = product.Volume,
                AnnualRevenue = product.Revenue,
                ContributionPercentage = Math.Round(contributionPercentage, 2),
                CumulativePercentage = Math.Round(cumulativePercentage, 2),
                Rank = rank++,
                LastCalculatedAt = DateTime.UtcNow,
                RecommendedStrategy = GetRecommendedStrategy(classification)
            });
        }

        // Group by classification
        var classA = classifiedProducts.Where(p => p.Classification == "A").ToList();
        var classB = classifiedProducts.Where(p => p.Classification == "B").ToList();
        var classC = classifiedProducts.Where(p => p.Classification == "C").ToList();

        // Calculate category breakdown
        var categoryBreakdown = classifiedProducts
            .GroupBy(p => p.CategoryName)
            .Select(g => new CategoryABCDto
            {
                CategoryName = g.Key,
                ClassACount = g.Count(p => p.Classification == "A"),
                ClassBCount = g.Count(p => p.Classification == "B"),
                ClassCCount = g.Count(p => p.Classification == "C"),
                TotalRevenue = g.Sum(p => p.AnnualRevenue)
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

        return new ABCAnalysisDto
        {
            Summary = new ABCAnalysisSummaryDto
            {
                TotalProducts = totalProducts,
                ClassACount = classA.Count,
                ClassBCount = classB.Count,
                ClassCCount = classC.Count,
                ClassARevenue = classA.Sum(p => p.AnnualRevenue),
                ClassBRevenue = classB.Sum(p => p.AnnualRevenue),
                ClassCRevenue = classC.Sum(p => p.AnnualRevenue),
                ClassAPercentage = Math.Round((decimal)classA.Count / totalProducts * 100, 2),
                ClassBPercentage = Math.Round((decimal)classB.Count / totalProducts * 100, 2),
                ClassCPercentage = Math.Round((decimal)classC.Count / totalProducts * 100, 2),
                TotalRevenue = totalRevenue
            },
            ClassAProducts = classA,
            ClassBProducts = classB,
            ClassCProducts = classC,
            ByCategory = categoryBreakdown,
            CalculatedAt = DateTime.UtcNow
        };
    }

    public string ClassifyProduct(decimal contributionPercentage, decimal cumulativePercentage)
    {
        // Pareto principle (80-15-5 rule)
        // Class A: Top items contributing to first 80% of revenue
        // Class B: Next items contributing to next 15% (80-95%)
        // Class C: Remaining items contributing to last 5% (95-100%)

        if (cumulativePercentage <= 80)
            return "A";
        else if (cumulativePercentage <= 95)
            return "B";
        else
            return "C";
    }

    public string GetRecommendedStrategy(string classification)
    {
        return classification switch
        {
            "A" => "High priority: Maintain optimal stock levels, frequent monitoring, strong supplier relationships, accurate demand forecasting",
            "B" => "Medium priority: Regular monitoring, balanced inventory levels, periodic review of demand patterns",
            "C" => "Low priority: Minimize inventory, consider discontinuation of slow movers, focus on efficient ordering",
            _ => "Unknown classification"
        };
    }
}
