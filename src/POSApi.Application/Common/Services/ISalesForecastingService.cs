using POSApi.Application.Common.DTOs.Analytics;

namespace POSApi.Application.Common.Services;

public interface ISalesForecastingService
{
    /// <summary>
    /// Generate sales forecast using linear regression
    /// </summary>
    Task<List<DailyForecastDto>> LinearRegressionForecast(Guid productId, Guid? storeId, int forecastDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate sales forecast using moving average
    /// </summary>
    Task<List<DailyForecastDto>> MovingAverageForecast(Guid productId, Guid? storeId, int forecastDays, int windowSize = 7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate sales forecast using exponential smoothing
    /// </summary>
    Task<List<DailyForecastDto>> ExponentialSmoothingForecast(Guid productId, Guid? storeId, int forecastDays, decimal alpha = 0.3m, CancellationToken cancellationToken = default);

    /// <summary>
    /// Auto-select best forecasting method based on historical data
    /// </summary>
    Task<(string Method, List<DailyForecastDto> Forecast)> SelectBestMethodAndForecast(Guid productId, Guid? storeId, int forecastDays, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate confidence interval for forecast
    /// </summary>
    decimal CalculateConfidence(List<decimal> historicalData, List<decimal> predictions);
}
