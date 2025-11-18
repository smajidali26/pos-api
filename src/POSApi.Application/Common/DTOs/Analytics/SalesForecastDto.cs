namespace POSApi.Application.Common.DTOs.Analytics;

public class SalesForecastDto
{
    public Guid Id { get; set; }
    public DateTime ForecastDate { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public decimal PredictedQuantity { get; set; }
    public decimal PredictedRevenue { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public string ForecastMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SalesForecastSummaryDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public List<DailyForecastDto> DailyForecasts { get; set; } = new();
    public decimal TotalPredictedRevenue { get; set; }
    public decimal TotalPredictedQuantity { get; set; }
    public decimal AverageConfidence { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

public class DailyForecastDto
{
    public DateTime Date { get; set; }
    public decimal PredictedQuantity { get; set; }
    public decimal PredictedRevenue { get; set; }
    public decimal ConfidenceLower { get; set; }
    public decimal ConfidenceUpper { get; set; }
}
