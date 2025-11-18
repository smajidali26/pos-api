using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Represents a sales forecast for a product
/// </summary>
public class SalesForecast : AggregateRoot
{
    public DateTime ForecastDate { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? StoreId { get; private set; }
    public Store? Store { get; private set; }
    public decimal PredictedQuantity { get; private set; }
    public decimal PredictedRevenue { get; private set; }
    public decimal ConfidenceLevel { get; private set; }
    public ForecastMethod ForecastMethod { get; private set; }
    public Dictionary<string, object> Metadata { get; private set; } = new();

    private SalesForecast() { } // For EF Core

    public SalesForecast(DateTime forecastDate, Guid productId, Guid? storeId,
                        decimal predictedQuantity, decimal predictedRevenue,
                        decimal confidenceLevel, ForecastMethod method)
    {
        ForecastDate = forecastDate;
        ProductId = productId;
        StoreId = storeId;
        PredictedQuantity = predictedQuantity;
        PredictedRevenue = predictedRevenue;
        ConfidenceLevel = confidenceLevel;
        ForecastMethod = method;
    }

    public void UpdateForecast(decimal predictedQuantity, decimal predictedRevenue, decimal confidenceLevel)
    {
        PredictedQuantity = predictedQuantity;
        PredictedRevenue = predictedRevenue;
        ConfidenceLevel = confidenceLevel;
        SetUpdatedAt();
    }

    public void AdjustConfidence(decimal newConfidenceLevel)
    {
        if (newConfidenceLevel < 0 || newConfidenceLevel > 1)
            throw new ArgumentException("Confidence level must be between 0 and 1", nameof(newConfidenceLevel));

        ConfidenceLevel = newConfidenceLevel;
        SetUpdatedAt();
    }

    public void AddMetadata(string key, object value)
    {
        Metadata[key] = value;
        SetUpdatedAt();
    }
}

/// <summary>
/// Represents ABC classification for inventory items based on Pareto principle
/// </summary>
public class ProductABCClassification : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? StoreId { get; private set; }
    public Store? Store { get; private set; }
    public ABCClass Classification { get; private set; }
    public decimal AnnualVolume { get; private set; }
    public decimal AnnualRevenue { get; private set; }
    public decimal ContributionPercentage { get; private set; }
    public DateTime LastCalculatedAt { get; private set; }
    public int Rank { get; private set; }
    public decimal CumulativePercentage { get; private set; }

    private ProductABCClassification() { } // For EF Core

    public ProductABCClassification(Guid productId, Guid? storeId, ABCClass classification,
                                   decimal annualVolume, decimal annualRevenue,
                                   decimal contributionPercentage, int rank, decimal cumulativePercentage)
    {
        ProductId = productId;
        StoreId = storeId;
        Classification = classification;
        AnnualVolume = annualVolume;
        AnnualRevenue = annualRevenue;
        ContributionPercentage = contributionPercentage;
        Rank = rank;
        CumulativePercentage = cumulativePercentage;
        LastCalculatedAt = DateTime.UtcNow;
    }

    public void Reclassify(ABCClass newClassification, decimal contributionPercentage,
                          int rank, decimal cumulativePercentage)
    {
        Classification = newClassification;
        ContributionPercentage = contributionPercentage;
        Rank = rank;
        CumulativePercentage = cumulativePercentage;
        LastCalculatedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void UpdateMetrics(decimal annualVolume, decimal annualRevenue)
    {
        AnnualVolume = annualVolume;
        AnnualRevenue = annualRevenue;
        LastCalculatedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}

/// <summary>
/// Stores inventory turnover metrics for products
/// </summary>
public class InventoryTurnover : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? StoreId { get; private set; }
    public Store? Store { get; private set; }
    public DateTime CalculationDate { get; private set; }
    public int PeriodDays { get; private set; }
    public decimal TurnoverRatio { get; private set; }
    public decimal DaysToSell { get; private set; }
    public decimal AverageCOGS { get; private set; }
    public decimal AverageInventoryValue { get; private set; }
    public TurnoverClassification Classification { get; private set; }

    private InventoryTurnover() { } // For EF Core

    public InventoryTurnover(Guid productId, Guid? storeId, int periodDays,
                            decimal turnoverRatio, decimal daysToSell,
                            decimal averageCOGS, decimal averageInventoryValue,
                            TurnoverClassification classification)
    {
        ProductId = productId;
        StoreId = storeId;
        CalculationDate = DateTime.UtcNow;
        PeriodDays = periodDays;
        TurnoverRatio = turnoverRatio;
        DaysToSell = daysToSell;
        AverageCOGS = averageCOGS;
        AverageInventoryValue = averageInventoryValue;
        Classification = classification;
    }

    public void UpdateMetrics(decimal turnoverRatio, decimal daysToSell,
                             decimal averageCOGS, decimal averageInventoryValue,
                             TurnoverClassification classification)
    {
        TurnoverRatio = turnoverRatio;
        DaysToSell = daysToSell;
        AverageCOGS = averageCOGS;
        AverageInventoryValue = averageInventoryValue;
        Classification = classification;
        CalculationDate = DateTime.UtcNow;
        SetUpdatedAt();
    }
}

/// <summary>
/// Forecast method used for sales predictions
/// </summary>
public enum ForecastMethod
{
    LinearRegression,
    MovingAverage,
    ExponentialSmoothing,
    SeasonalDecomposition,
    WeightedAverage,
    AutoSelected
}

/// <summary>
/// ABC Classification based on Pareto principle
/// </summary>
public enum ABCClass
{
    /// <summary>Top 20% of products contributing 80% of revenue</summary>
    A,
    /// <summary>Next 30% of products contributing 15% of revenue</summary>
    B,
    /// <summary>Remaining 50% of products contributing 5% of revenue</summary>
    C
}

/// <summary>
/// Inventory turnover classification
/// </summary>
public enum TurnoverClassification
{
    /// <summary>Turnover ratio greater than 12 times per year</summary>
    Fast,
    /// <summary>Turnover ratio between 4-12 times per year</summary>
    Normal,
    /// <summary>Turnover ratio less than 4 times per year</summary>
    Slow,
    /// <summary>No sales in the analysis period</summary>
    Dead
}
