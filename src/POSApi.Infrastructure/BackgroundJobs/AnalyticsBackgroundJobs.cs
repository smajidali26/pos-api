using Microsoft.Extensions.Logging;
using POSApi.Application.Common.Services;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace POSApi.Infrastructure.BackgroundJobs;

public class AnalyticsBackgroundJobs : IAnalyticsBackgroundJobs
{
    private readonly PosDbContext _context;
    private readonly ISalesForecastingService _forecastingService;
    private readonly IABCAnalysisService _abcAnalysisService;
    private readonly IInventoryTurnoverService _turnoverService;
    private readonly ILogger<AnalyticsBackgroundJobs> _logger;

    public AnalyticsBackgroundJobs(
        PosDbContext context,
        ISalesForecastingService forecastingService,
        IABCAnalysisService abcAnalysisService,
        IInventoryTurnoverService turnoverService,
        ILogger<AnalyticsBackgroundJobs> logger)
    {
        _context = context;
        _forecastingService = forecastingService;
        _abcAnalysisService = abcAnalysisService;
        _turnoverService = turnoverService;
        _logger = logger;
    }

    public async Task GenerateDailySalesForecastsAsync()
    {
        _logger.LogInformation("Starting daily sales forecasts generation at {Time}", DateTime.UtcNow);

        try
        {
            // Get all active products
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            _logger.LogInformation("Generating forecasts for {Count} products", products.Count);

            var forecastsGenerated = 0;
            var forecastsFailed = 0;

            foreach (var product in products)
            {
                try
                {
                    // Get historical sales data for the last 90 days
                    var historicalData = await _context.OrderItems
                        .Where(oi => oi.ProductId == product.Id &&
                                    oi.Order.OrderDate >= DateTime.UtcNow.AddDays(-90))
                        .GroupBy(oi => oi.Order.OrderDate.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            Quantity = g.Sum(oi => oi.Quantity)
                        })
                        .OrderBy(d => d.Date)
                        .ToListAsync();

                    if (historicalData.Count < 7) // Need at least 7 days of data
                    {
                        _logger.LogDebug("Skipping product {ProductId} - insufficient historical data ({Days} days)",
                            product.Id, historicalData.Count);
                        continue;
                    }

                    // Convert to service format
                    var salesData = historicalData.Select((d, index) => new SalesDataPoint
                    {
                        Date = d.Date,
                        Quantity = d.Quantity,
                        DayIndex = index + 1
                    }).ToList();

                    // Auto-select best forecasting method
                    var (method, forecasts) = _forecastingService.AutoSelectBestMethod(salesData, 30);

                    // Save forecasts to database
                    foreach (var forecast in forecasts)
                    {
                        var existingForecast = await _context.SalesForecasts
                            .FirstOrDefaultAsync(sf => sf.ProductId == product.Id &&
                                                      sf.ForecastDate == forecast.Date);

                        if (existingForecast != null)
                        {
                            // Update existing forecast
                            existingForecast.UpdateForecast(
                                forecast.PredictedQuantity,
                                method,
                                forecast.ConfidenceLevel
                            );
                        }
                        else
                        {
                            // Create new forecast
                            var newForecast = new SalesForecast(
                                product.Id,
                                forecast.Date,
                                forecast.PredictedQuantity,
                                method,
                                forecast.ConfidenceLevel
                            );
                            _context.SalesForecasts.Add(newForecast);
                        }
                    }

                    await _context.SaveChangesAsync();
                    forecastsGenerated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error generating forecast for product {ProductId}", product.Id);
                    forecastsFailed++;
                }
            }

            _logger.LogInformation(
                "Daily forecasts generation completed. Generated: {Generated}, Failed: {Failed}",
                forecastsGenerated, forecastsFailed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error during daily forecasts generation");
            throw;
        }
    }

    public async Task RecalculateABCClassificationAsync()
    {
        _logger.LogInformation("Starting ABC classification recalculation at {Time}", DateTime.UtcNow);

        try
        {
            // Get all products with their annual sales
            var productSalesData = await _context.OrderItems
                .Where(oi => oi.Order.OrderDate >= DateTime.UtcNow.AddYears(-1))
                .GroupBy(oi => oi.ProductId)
                .Select(g => new ProductSalesData
                {
                    ProductId = g.Key,
                    AnnualRevenue = g.Sum(oi => oi.TotalPrice),
                    AnnualQuantitySold = g.Sum(oi => oi.Quantity)
                })
                .ToListAsync();

            _logger.LogInformation("Calculating ABC classification for {Count} products", productSalesData.Count);

            var result = _abcAnalysisService.CalculateABCClassification(productSalesData);

            // Update or create classifications in database
            foreach (var classification in result.ClassifiedProducts)
            {
                var existing = await _context.ProductABCClassifications
                    .FirstOrDefaultAsync(c => c.ProductId == classification.ProductId);

                if (existing != null)
                {
                    existing.UpdateClassification(
                        classification.Classification,
                        classification.ContributionPercentage,
                        classification.CumulativePercentage
                    );
                }
                else
                {
                    var newClassification = new ProductABCClassification(
                        classification.ProductId,
                        classification.Classification,
                        classification.ContributionPercentage,
                        classification.CumulativePercentage
                    );
                    _context.ProductABCClassifications.Add(newClassification);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("ABC classification completed. Class A: {A}, Class B: {B}, Class C: {C}",
                result.ClassifiedProducts.Count(p => p.Classification == ABCClass.A),
                result.ClassifiedProducts.Count(p => p.Classification == ABCClass.B),
                result.ClassifiedProducts.Count(p => p.Classification == ABCClass.C));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during ABC classification recalculation");
            throw;
        }
    }

    public async Task CalculateInventoryTurnoverAsync()
    {
        _logger.LogInformation("Starting inventory turnover calculation at {Time}", DateTime.UtcNow);

        try
        {
            var products = await _context.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            _logger.LogInformation("Calculating turnover for {Count} products", products.Count);

            var turnoversCalculated = 0;

            foreach (var product in products)
            {
                try
                {
                    // Get COGS for the period (last 365 days)
                    var cogs = await _context.OrderItems
                        .Where(oi => oi.ProductId == product.Id &&
                                    oi.Order.OrderDate >= DateTime.UtcNow.AddYears(-1))
                        .SumAsync(oi => oi.Quantity * oi.UnitPrice);

                    // Get average inventory value
                    // Simplified: using current stock * cost price as average
                    var averageInventoryValue = product.Stock * product.CostPrice;

                    if (averageInventoryValue == 0)
                    {
                        _logger.LogDebug("Skipping product {ProductId} - zero inventory value", product.Id);
                        continue;
                    }

                    var turnoverRatio = _turnoverService.CalculateTurnoverRatio(cogs, averageInventoryValue);
                    var daysInventory = _turnoverService.CalculateDaysOfInventory(turnoverRatio);
                    var classification = _turnoverService.ClassifyStockMovement(turnoverRatio);

                    // Update or create turnover record
                    var existing = await _context.InventoryTurnovers
                        .FirstOrDefaultAsync(it => it.ProductId == product.Id);

                    if (existing != null)
                    {
                        existing.UpdateTurnover(
                            turnoverRatio,
                            daysInventory,
                            classification,
                            cogs,
                            averageInventoryValue
                        );
                    }
                    else
                    {
                        var newTurnover = new InventoryTurnover(
                            product.Id,
                            turnoverRatio,
                            daysInventory,
                            classification,
                            cogs,
                            averageInventoryValue
                        );
                        _context.InventoryTurnovers.Add(newTurnover);
                    }

                    turnoversCalculated++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating turnover for product {ProductId}", product.Id);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Inventory turnover calculation completed for {Count} products",
                turnoversCalculated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during inventory turnover calculation");
            throw;
        }
    }

    public async Task CleanupOldForecastsAsync()
    {
        _logger.LogInformation("Starting cleanup of old forecasts at {Time}", DateTime.UtcNow);

        try
        {
            // Delete forecasts older than 90 days
            var cutoffDate = DateTime.UtcNow.AddDays(-90);

            var oldForecasts = await _context.SalesForecasts
                .Where(sf => sf.ForecastDate < cutoffDate)
                .ToListAsync();

            if (oldForecasts.Any())
            {
                _context.SalesForecasts.RemoveRange(oldForecasts);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} old forecast records", oldForecasts.Count);
            }
            else
            {
                _logger.LogInformation("No old forecasts to clean up");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forecast cleanup");
            throw;
        }
    }
}
