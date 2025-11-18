using FluentValidation;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.Services;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Analytics.Queries.GetSalesForecast;

public class GetSalesForecastQuery : IQuery<SalesForecastSummaryDto>
{
    public Guid? ProductId { get; set; }
    public Guid? StoreId { get; set; }
    public int ForecastDays { get; set; } = 30;
    public string? Method { get; set; } // LinearRegression, MovingAverage, ExponentialSmoothing, Auto

    public GetSalesForecastQuery() { }

    public GetSalesForecastQuery(Guid? productId, Guid? storeId, int forecastDays, string? method)
    {
        ProductId = productId;
        StoreId = storeId;
        ForecastDays = forecastDays;
        Method = method;
    }
}

public class GetSalesForecastQueryValidator : AbstractValidator<GetSalesForecastQuery>
{
    public GetSalesForecastQueryValidator()
    {
        RuleFor(x => x.ForecastDays)
            .GreaterThan(0).WithMessage("Forecast days must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Forecast days cannot exceed 365");

        RuleFor(x => x.Method)
            .Must(m => string.IsNullOrEmpty(m) || new[] { "LinearRegression", "MovingAverage", "ExponentialSmoothing", "Auto" }.Contains(m))
            .WithMessage("Invalid forecast method. Use: LinearRegression, MovingAverage, ExponentialSmoothing, or Auto");
    }
}

public class GetSalesForecastQueryHandler : IQueryHandler<GetSalesForecastQuery, SalesForecastSummaryDto>
{
    private readonly ISalesForecastingService _forecastingService;
    private readonly IUnitOfWork _unitOfWork;

    public GetSalesForecastQueryHandler(ISalesForecastingService forecastingService, IUnitOfWork unitOfWork)
    {
        _forecastingService = forecastingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesForecastSummaryDto> Handle(GetSalesForecastQuery request, CancellationToken cancellationToken)
    {
        if (!request.ProductId.HasValue)
        {
            throw new ArgumentException("ProductId is required for sales forecast");
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId.Value, cancellationToken);
        if (product == null)
        {
            throw new KeyNotFoundException($"Product with ID {request.ProductId} not found");
        }

        List<DailyForecastDto> forecasts;
        string methodUsed;

        if (string.IsNullOrEmpty(request.Method) || request.Method.Equals("Auto", StringComparison.OrdinalIgnoreCase))
        {
            var result = await _forecastingService.SelectBestMethodAndForecast(
                request.ProductId.Value, request.StoreId, request.ForecastDays, cancellationToken);
            forecasts = result.Forecast;
            methodUsed = result.Method;
        }
        else
        {
            methodUsed = request.Method;
            forecasts = request.Method.ToLower() switch
            {
                "linearregression" => await _forecastingService.LinearRegressionForecast(
                    request.ProductId.Value, request.StoreId, request.ForecastDays, cancellationToken),
                "movingaverage" => await _forecastingService.MovingAverageForecast(
                    request.ProductId.Value, request.StoreId, request.ForecastDays, 7, cancellationToken),
                "exponentialsmoothing" => await _forecastingService.ExponentialSmoothingForecast(
                    request.ProductId.Value, request.StoreId, request.ForecastDays, 0.3m, cancellationToken),
                _ => throw new ArgumentException($"Unknown forecast method: {request.Method}")
            };
        }

        var totalPredictedRevenue = forecasts.Sum(f => f.PredictedRevenue);
        var totalPredictedQuantity = forecasts.Sum(f => f.PredictedQuantity);
        var avgConfidence = forecasts.Any() ? forecasts.Average(f => (f.ConfidenceUpper - f.ConfidenceLower) / 2) : 0;

        // Generate recommendation
        var recommendation = GenerateRecommendation(totalPredictedQuantity, product.StockQuantity,
            product.ReorderLevel, request.ForecastDays);

        return new SalesForecastSummaryDto
        {
            ProductId = product.Id,
            ProductName = product.Name,
            SKU = product.SKU,
            DailyForecasts = forecasts,
            TotalPredictedRevenue = Math.Round(totalPredictedRevenue, 2),
            TotalPredictedQuantity = Math.Round(totalPredictedQuantity, 2),
            AverageConfidence = Math.Round(avgConfidence, 2),
            RecommendedAction = recommendation
        };
    }

    private string GenerateRecommendation(decimal predictedQuantity, int currentStock, int reorderLevel, int days)
    {
        if (predictedQuantity > currentStock)
        {
            var deficit = predictedQuantity - currentStock;
            return $"REORDER RECOMMENDED: Predicted demand ({predictedQuantity:F0} units) exceeds current stock ({currentStock} units). " +
                   $"Order at least {deficit:F0} units to meet forecasted demand for the next {days} days.";
        }
        else if (currentStock < reorderLevel)
        {
            return $"STOCK LOW: Current stock ({currentStock} units) is below reorder level ({reorderLevel} units). " +
                   $"Consider reordering even though forecasted demand is {predictedQuantity:F0} units.";
        }
        else
        {
            var daysOfSupply = predictedQuantity > 0 ? (currentStock / (predictedQuantity / days)) : 999;
            return $"STOCK ADEQUATE: Current stock ({currentStock} units) should cover approximately {daysOfSupply:F0} days " +
                   $"based on predicted demand of {predictedQuantity:F0} units over {days} days.";
        }
    }
}
