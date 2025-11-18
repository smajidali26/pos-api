using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Application.Common.Services;
using POSApi.Application.Features.Analytics.Commands.CalculateABCClassification;
using POSApi.Application.Features.Analytics.Commands.GenerateSalesForecast;
using POSApi.Application.Features.Analytics.Queries.GetABCAnalysis;
using POSApi.Application.Features.Analytics.Queries.GetInventoryTurnover;
using POSApi.Application.Features.Analytics.Queries.GetRealTimeDashboard;
using POSApi.Application.Features.Analytics.Queries.GetSalesAnalytics;
using POSApi.Application.Features.Analytics.Queries.GetSalesForecast;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IInventoryTurnoverService _inventoryTurnoverService;

    public AnalyticsController(IMediator mediator, IInventoryTurnoverService inventoryTurnoverService)
    {
        _mediator = mediator;
        _inventoryTurnoverService = inventoryTurnoverService;
    }

    /// <summary>
    /// Get real-time dashboard metrics
    /// </summary>
    [HttpGet("dashboard/realtime")]
    [ProducesResponseType(typeof(RealTimeDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<RealTimeDashboardDto>> GetRealTimeDashboard([FromQuery] Guid? storeId = null)
    {
        var query = new GetRealTimeDashboardQuery(storeId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get comprehensive sales analytics for a date range
    /// </summary>
    [HttpGet("sales")]
    [ProducesResponseType(typeof(SalesAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] Guid? storeId = null,
        [FromQuery] string groupBy = "day")
    {
        var query = new GetSalesAnalyticsQuery
        {
            StartDate = startDate,
            EndDate = endDate,
            StoreId = storeId,
            GroupBy = groupBy
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get sales forecast for a product
    /// </summary>
    [HttpGet("sales/forecast")]
    [ProducesResponseType(typeof(SalesForecastSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SalesForecastSummaryDto>> GetSalesForecast(
        [FromQuery] Guid productId,
        [FromQuery] Guid? storeId = null,
        [FromQuery] int forecastDays = 30,
        [FromQuery] string? method = null)
    {
        var query = new GetSalesForecastQuery(productId, storeId, forecastDays, method);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Generate and save sales forecasts for products
    /// </summary>
    [HttpPost("sales/forecast/generate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> GenerateSalesForecast([FromBody] GenerateSalesForecastCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get inventory turnover analysis
    /// </summary>
    [HttpGet("inventory/turnover")]
    [ProducesResponseType(typeof(InventoryTurnoverSummaryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryTurnoverSummaryDto>> GetInventoryTurnover(
        [FromQuery] Guid? storeId = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] int periodDays = 365)
    {
        var query = new GetInventoryTurnoverQuery
        {
            StoreId = storeId,
            CategoryId = categoryId,
            PeriodDays = periodDays
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get ABC analysis for inventory classification
    /// </summary>
    [HttpGet("inventory/abc-analysis")]
    [ProducesResponseType(typeof(ABCAnalysisDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ABCAnalysisDto>> GetABCAnalysis(
        [FromQuery] Guid? storeId = null,
        [FromQuery] int periodDays = 365)
    {
        var query = new GetABCAnalysisQuery
        {
            StoreId = storeId,
            PeriodDays = periodDays
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Calculate and save ABC classification for products
    /// </summary>
    [HttpPost("inventory/abc-analysis/calculate")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> CalculateABCClassification([FromBody] CalculateABCClassificationCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Get slow-moving inventory items
    /// </summary>
    [HttpGet("inventory/slow-moving")]
    [ProducesResponseType(typeof(List<InventoryTurnoverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<InventoryTurnoverDto>>> GetSlowMovingStock([FromQuery] Guid? storeId = null)
    {
        var result = await _inventoryTurnoverService.IdentifySlowMovingStock(storeId);
        return Ok(result);
    }

    /// <summary>
    /// Get reorder recommendations based on turnover analysis
    /// </summary>
    [HttpGet("inventory/reorder-recommendations")]
    [ProducesResponseType(typeof(List<ReorderRecommendationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReorderRecommendationDto>>> GetReorderRecommendations([FromQuery] Guid? storeId = null)
    {
        var result = await _inventoryTurnoverService.GenerateReorderRecommendations(storeId);
        return Ok(result);
    }

    /// <summary>
    /// Get inventory analytics summary
    /// </summary>
    [HttpGet("inventory")]
    [ProducesResponseType(typeof(InventoryAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InventoryAnalyticsDto>> GetInventoryAnalytics([FromQuery] Guid? storeId = null)
    {
        // Get turnover data
        var turnoverData = await _inventoryTurnoverService.CalculateTurnoverRatio(storeId);

        // Get reorder recommendations
        var reorderRecommendations = await _inventoryTurnoverService.GenerateReorderRecommendations(storeId);

        // Build analytics DTO
        var analytics = new InventoryAnalyticsDto
        {
            StoreId = storeId,
            TotalInventoryValue = turnoverData.TotalInventoryValue,
            AverageTurnoverRate = turnoverData.OverallTurnoverRatio,
            TotalProducts = turnoverData.Items.Count,
            LowStockProducts = reorderRecommendations.Count,
            OutOfStockProducts = turnoverData.Items.Count(i => i.CurrentStock == 0),
            OverstockedProducts = turnoverData.Items.Count(i => i.EstimatedMonthsOfSupply > 6),
            DeadStockProducts = turnoverData.DeadStockItems,
            HealthScore = CalculateInventoryHealthScore(turnoverData),
            CarryingCost = turnoverData.TotalInventoryValue * 0.15m, // Assume 15% annual carrying cost
            ObsolescenceRisk = CalculateObsolescenceRisk(turnoverData),
            ByCategory = turnoverData.ByCategory.Select(c => new CategoryInventoryDto
            {
                CategoryId = Guid.Empty, // Would need to fetch from category data
                CategoryName = c.CategoryName,
                InventoryValue = c.TotalInventoryValue,
                ProductCount = c.ProductCount,
                AverageTurnover = c.AverageTurnoverRatio,
                LowStockCount = 0
            }).ToList(),
            ReorderRecommendations = reorderRecommendations,
            OverstockedItems = turnoverData.Items
                .Where(i => i.EstimatedMonthsOfSupply > 6)
                .Select(i => new OverstockedItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    SKU = i.SKU,
                    CurrentStock = i.CurrentStock,
                    AverageDailySales = i.DaysToSell > 0 ? i.CurrentStock / i.DaysToSell : 0,
                    MonthsOfSupply = i.EstimatedMonthsOfSupply,
                    ExcessQuantity = 0, // Would need more calculation
                    TiedUpCapital = i.AverageInventoryValue,
                    Recommendation = i.Recommendation
                }).ToList(),
            DeadStockItems = turnoverData.Items
                .Where(i => i.Classification == "Dead")
                .Select(i => new DeadStockItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    SKU = i.SKU,
                    CurrentStock = i.CurrentStock,
                    LastSaleDate = null, // Would need to fetch from orders
                    DaysSinceLastSale = 365,
                    InventoryValue = i.AverageInventoryValue,
                    Recommendation = i.Recommendation
                }).ToList(),
            CalculatedAt = DateTime.UtcNow
        };

        return Ok(analytics);
    }

    private decimal CalculateInventoryHealthScore(InventoryTurnoverSummaryDto turnoverData)
    {
        // Calculate health score based on various factors (0-100 scale)
        var fastMovingPercentage = turnoverData.Items.Count > 0
            ? (decimal)turnoverData.FastMovingItems / turnoverData.Items.Count * 100
            : 0;

        var deadStockPercentage = turnoverData.Items.Count > 0
            ? (decimal)turnoverData.DeadStockItems / turnoverData.Items.Count * 100
            : 0;

        var healthScore = (fastMovingPercentage * 0.5m) + ((100 - deadStockPercentage) * 0.5m);

        return Math.Round(Math.Max(0, Math.Min(100, healthScore)), 2);
    }

    private decimal CalculateObsolescenceRisk(InventoryTurnoverSummaryDto turnoverData)
    {
        // Calculate risk of obsolescence based on slow and dead stock
        var totalValue = turnoverData.TotalInventoryValue;
        if (totalValue == 0) return 0;

        var riskValue = turnoverData.Items
            .Where(i => i.Classification == "Slow" || i.Classification == "Dead")
            .Sum(i => i.AverageInventoryValue);

        return Math.Round((riskValue / totalValue) * 100, 2);
    }
}
