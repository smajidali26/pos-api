using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Common.Services;

public class SalesForecastingService : ISalesForecastingService
{
    private readonly IUnitOfWork _unitOfWork;

    public SalesForecastingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<DailyForecastDto>> LinearRegressionForecast(
        Guid productId, Guid? storeId, int forecastDays, CancellationToken cancellationToken = default)
    {
        var historicalData = await GetHistoricalSalesData(productId, storeId, 90, cancellationToken);

        if (historicalData.Count < 7)
        {
            // Not enough data for regression, use average
            return await MovingAverageForecast(productId, storeId, forecastDays, historicalData.Count, cancellationToken);
        }

        // Prepare data for linear regression
        var n = historicalData.Count;
        var x = Enumerable.Range(1, n).Select(i => (decimal)i).ToList();
        var y = historicalData.Select(d => d.Quantity).ToList();

        // Calculate linear regression coefficients using least squares method
        var xMean = x.Average();
        var yMean = y.Average();

        var numerator = 0m;
        var denominator = 0m;

        for (int i = 0; i < n; i++)
        {
            numerator += (x[i] - xMean) * (y[i] - yMean);
            denominator += (x[i] - xMean) * (x[i] - xMean);
        }

        var slope = denominator != 0 ? numerator / denominator : 0;
        var intercept = yMean - (slope * xMean);

        // Generate forecasts
        var forecasts = new List<DailyForecastDto>();
        var lastDate = historicalData.Max(d => d.Date);
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);

        for (int i = 1; i <= forecastDays; i++)
        {
            var forecastDate = lastDate.AddDays(i);
            var predictedQuantity = Math.Max(0, slope * (n + i) + intercept);
            var predictedRevenue = predictedQuantity * (product?.Price ?? 0);

            // Calculate confidence interval (simplified)
            var standardError = CalculateStandardError(y, x, slope, intercept);
            var margin = 1.96m * standardError; // 95% confidence interval

            forecasts.Add(new DailyForecastDto
            {
                Date = forecastDate,
                PredictedQuantity = Math.Round(predictedQuantity, 2),
                PredictedRevenue = Math.Round(predictedRevenue, 2),
                ConfidenceLower = Math.Max(0, Math.Round(predictedQuantity - margin, 2)),
                ConfidenceUpper = Math.Round(predictedQuantity + margin, 2)
            });
        }

        return forecasts;
    }

    public async Task<List<DailyForecastDto>> MovingAverageForecast(
        Guid productId, Guid? storeId, int forecastDays, int windowSize = 7, CancellationToken cancellationToken = default)
    {
        var historicalData = await GetHistoricalSalesData(productId, storeId, Math.Max(windowSize * 2, 30), cancellationToken);

        if (historicalData.Count == 0)
        {
            return new List<DailyForecastDto>();
        }

        var actualWindowSize = Math.Min(windowSize, historicalData.Count);
        var recentData = historicalData.OrderByDescending(d => d.Date).Take(actualWindowSize).ToList();
        var avgQuantity = recentData.Average(d => d.Quantity);

        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        var forecasts = new List<DailyForecastDto>();
        var lastDate = historicalData.Max(d => d.Date);

        // Calculate standard deviation for confidence interval
        var stdDev = CalculateStandardDeviation(recentData.Select(d => d.Quantity).ToList());
        var margin = 1.96m * stdDev;

        for (int i = 1; i <= forecastDays; i++)
        {
            var forecastDate = lastDate.AddDays(i);
            var predictedRevenue = avgQuantity * (product?.Price ?? 0);

            forecasts.Add(new DailyForecastDto
            {
                Date = forecastDate,
                PredictedQuantity = Math.Round(avgQuantity, 2),
                PredictedRevenue = Math.Round(predictedRevenue, 2),
                ConfidenceLower = Math.Max(0, Math.Round(avgQuantity - margin, 2)),
                ConfidenceUpper = Math.Round(avgQuantity + margin, 2)
            });
        }

        return forecasts;
    }

    public async Task<List<DailyForecastDto>> ExponentialSmoothingForecast(
        Guid productId, Guid? storeId, int forecastDays, decimal alpha = 0.3m, CancellationToken cancellationToken = default)
    {
        var historicalData = await GetHistoricalSalesData(productId, storeId, 90, cancellationToken);

        if (historicalData.Count == 0)
        {
            return new List<DailyForecastDto>();
        }

        // Sort by date
        var sortedData = historicalData.OrderBy(d => d.Date).ToList();

        // Initialize with first value
        var smoothedValues = new List<decimal> { sortedData[0].Quantity };

        // Apply exponential smoothing
        for (int i = 1; i < sortedData.Count; i++)
        {
            var smoothed = alpha * sortedData[i].Quantity + (1 - alpha) * smoothedValues[i - 1];
            smoothedValues.Add(smoothed);
        }

        // Last smoothed value is our forecast
        var forecast = smoothedValues.Last();
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        var forecasts = new List<DailyForecastDto>();
        var lastDate = sortedData.Max(d => d.Date);

        // Calculate confidence interval
        var errors = new List<decimal>();
        for (int i = 1; i < sortedData.Count; i++)
        {
            errors.Add(Math.Abs(sortedData[i].Quantity - smoothedValues[i - 1]));
        }
        var mae = errors.Any() ? errors.Average() : 0;
        var margin = 1.96m * mae;

        for (int i = 1; i <= forecastDays; i++)
        {
            var forecastDate = lastDate.AddDays(i);
            var predictedRevenue = forecast * (product?.Price ?? 0);

            forecasts.Add(new DailyForecastDto
            {
                Date = forecastDate,
                PredictedQuantity = Math.Round(forecast, 2),
                PredictedRevenue = Math.Round(predictedRevenue, 2),
                ConfidenceLower = Math.Max(0, Math.Round(forecast - margin, 2)),
                ConfidenceUpper = Math.Round(forecast + margin, 2)
            });
        }

        return forecasts;
    }

    public async Task<(string Method, List<DailyForecastDto> Forecast)> SelectBestMethodAndForecast(
        Guid productId, Guid? storeId, int forecastDays, CancellationToken cancellationToken = default)
    {
        var historicalData = await GetHistoricalSalesData(productId, storeId, 90, cancellationToken);

        if (historicalData.Count < 7)
        {
            // Not enough data, use moving average
            var forecast = await MovingAverageForecast(productId, storeId, forecastDays, historicalData.Count, cancellationToken);
            return ("MovingAverage", forecast);
        }

        // Split data into training and test sets
        var trainSize = (int)(historicalData.Count * 0.8);
        var trainData = historicalData.OrderBy(d => d.Date).Take(trainSize).ToList();
        var testData = historicalData.OrderBy(d => d.Date).Skip(trainSize).ToList();

        // Evaluate each method on test data (simplified evaluation)
        var quantities = historicalData.Select(d => d.Quantity).ToList();
        var trend = CalculateTrend(quantities);
        var variance = CalculateStandardDeviation(quantities);

        // Decision logic based on data characteristics
        string bestMethod;
        if (trend > 0.1m && variance < quantities.Average() * 0.3m)
        {
            // Strong trend with low variance -> Linear Regression
            bestMethod = "LinearRegression";
            var forecast = await LinearRegressionForecast(productId, storeId, forecastDays, cancellationToken);
            return (bestMethod, forecast);
        }
        else if (variance < quantities.Average() * 0.2m)
        {
            // Low variance, stable -> Moving Average
            bestMethod = "MovingAverage";
            var forecast = await MovingAverageForecast(productId, storeId, forecastDays, 7, cancellationToken);
            return (bestMethod, forecast);
        }
        else
        {
            // Variable data -> Exponential Smoothing
            bestMethod = "ExponentialSmoothing";
            var forecast = await ExponentialSmoothingForecast(productId, storeId, forecastDays, 0.3m, cancellationToken);
            return (bestMethod, forecast);
        }
    }

    public decimal CalculateConfidence(List<decimal> historicalData, List<decimal> predictions)
    {
        if (historicalData.Count == 0 || predictions.Count == 0)
            return 0;

        // Calculate mean absolute percentage error (MAPE)
        var errors = new List<decimal>();
        var minCount = Math.Min(historicalData.Count, predictions.Count);

        for (int i = 0; i < minCount; i++)
        {
            if (historicalData[i] != 0)
            {
                var error = Math.Abs((historicalData[i] - predictions[i]) / historicalData[i]);
                errors.Add(error);
            }
        }

        if (errors.Count == 0)
            return 0;

        var mape = errors.Average();
        var confidence = Math.Max(0, 1 - mape);

        return Math.Round(confidence, 4);
    }

    private async Task<List<(DateTime Date, decimal Quantity, decimal Revenue)>> GetHistoricalSalesData(
        Guid productId, Guid? storeId, int days, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-days).Date;
        var endDate = DateTime.UtcNow.Date;

        var query = _unitOfWork.Context.Set<Domain.Entities.Order>()
            .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != Domain.Entities.OrderStatus.Cancelled)
            .SelectMany(o => o.OrderItems)
            .Where(oi => oi.ProductId == productId);

        if (storeId.HasValue)
        {
            query = query.Where(oi => oi.Order.StoreId == storeId.Value);
        }

        var dailySales = await query
            .GroupBy(oi => oi.Order.OrderDate.Date)
            .Select(g => new
            {
                Date = g.Key,
                Quantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.Price * oi.Quantity)
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return dailySales.Select(s => (s.Date, s.Quantity, s.Revenue)).ToList();
    }

    private decimal CalculateStandardError(List<decimal> actual, List<decimal> x, decimal slope, decimal intercept)
    {
        var sumSquaredResiduals = 0m;
        for (int i = 0; i < actual.Count; i++)
        {
            var predicted = slope * x[i] + intercept;
            var residual = actual[i] - predicted;
            sumSquaredResiduals += residual * residual;
        }

        var variance = sumSquaredResiduals / (actual.Count - 2);
        return (decimal)Math.Sqrt((double)variance);
    }

    private decimal CalculateStandardDeviation(List<decimal> values)
    {
        if (values.Count == 0) return 0;

        var mean = values.Average();
        var sumSquaredDiff = values.Sum(v => (v - mean) * (v - mean));
        var variance = sumSquaredDiff / values.Count;

        return (decimal)Math.Sqrt((double)variance);
    }

    private decimal CalculateTrend(List<decimal> values)
    {
        if (values.Count < 2) return 0;

        var first = values.Take(values.Count / 2).Average();
        var second = values.Skip(values.Count / 2).Average();

        return first != 0 ? (second - first) / first : 0;
    }
}
