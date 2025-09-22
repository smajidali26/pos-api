using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class AnalyticsReport : AggregateRoot
{
    public string ReportName { get; private set; } = string.Empty;
    public ReportType Type { get; private set; }
    public DateTime ReportDate { get; private set; }
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public Dictionary<string, object> Data { get; private set; } = new();
    public Dictionary<string, decimal> Metrics { get; private set; } = new();
    public ReportStatus Status { get; private set; }
    public Guid GeneratedByUserId { get; private set; }
    public User GeneratedBy { get; private set; } = null!;
    public string? FilePath { get; private set; }
    public int Version { get; private set; }

    private AnalyticsReport() { } // For EF Core

    public AnalyticsReport(string reportName, ReportType type, DateTime periodStart, DateTime periodEnd, Guid generatedByUserId)
    {
        ReportName = reportName;
        Type = type;
        ReportDate = DateTime.UtcNow;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
        GeneratedByUserId = generatedByUserId;
        Status = ReportStatus.Generating;
        Version = 1;
    }

    public void AddMetric(string key, decimal value)
    {
        Metrics[key] = value;
        SetUpdatedAt();
    }

    public void AddData(string key, object value)
    {
        Data[key] = value;
        SetUpdatedAt();
    }

    public void Complete(string? filePath = null)
    {
        Status = ReportStatus.Completed;
        FilePath = filePath;
        SetUpdatedAt();
    }

    public void Fail(string error)
    {
        Status = ReportStatus.Failed;
        AddData("Error", error);
        SetUpdatedAt();
    }
}

public class SalesAnalytics : BaseEntity
{
    public DateTime AnalysisDate { get; private set; }
    public Guid? StoreId { get; private set; }
    public Store? Store { get; private set; }
    public Guid? ProductId { get; private set; }
    public Product? Product { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public decimal TotalSales { get; private set; }
    public int TransactionCount { get; private set; }
    public decimal AverageTransactionValue { get; private set; }
    public int QuantitySold { get; private set; }
    public decimal RevenuePerSqFt { get; private set; }
    public decimal ConversionRate { get; private set; }
    public decimal ProfitMargin { get; private set; }

    private SalesAnalytics() { } // For EF Core

    public SalesAnalytics(DateTime analysisDate, decimal totalSales, int transactionCount, int quantitySold)
    {
        AnalysisDate = analysisDate;
        TotalSales = totalSales;
        TransactionCount = transactionCount;
        QuantitySold = quantitySold;
        AverageTransactionValue = transactionCount > 0 ? totalSales / transactionCount : 0;
    }

    public void UpdateMetrics(decimal revenuePerSqFt, decimal conversionRate, decimal profitMargin)
    {
        RevenuePerSqFt = revenuePerSqFt;
        ConversionRate = conversionRate;
        ProfitMargin = profitMargin;
        SetUpdatedAt();
    }
}

public class CustomerAnalytics : BaseEntity
{
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;
    public DateTime AnalysisDate { get; private set; }
    public decimal LifetimeValue { get; private set; }
    public decimal AverageOrderValue { get; private set; }
    public int PurchaseFrequency { get; private set; }
    public int DaysSinceLastPurchase { get; private set; }
    public CustomerSegment Segment { get; private set; }
    public decimal ChurnProbability { get; private set; }
    public List<string> PreferredCategories { get; private set; } = new();
    public decimal SeasonalityIndex { get; private set; }

    private CustomerAnalytics() { } // For EF Core

    public CustomerAnalytics(Guid customerId, DateTime analysisDate)
    {
        CustomerId = customerId;
        AnalysisDate = analysisDate;
    }

    public void UpdateAnalytics(decimal lifetimeValue, decimal averageOrderValue, int purchaseFrequency, 
                               int daysSinceLastPurchase, CustomerSegment segment, decimal churnProbability)
    {
        LifetimeValue = lifetimeValue;
        AverageOrderValue = averageOrderValue;
        PurchaseFrequency = purchaseFrequency;
        DaysSinceLastPurchase = daysSinceLastPurchase;
        Segment = segment;
        ChurnProbability = churnProbability;
        SetUpdatedAt();
    }
}

public class PredictiveAnalytics : BaseEntity
{
    public string ModelName { get; private set; } = string.Empty;
    public PredictionType Type { get; private set; }
    public DateTime PredictionDate { get; private set; }
    public DateTime TargetDate { get; private set; }
    public Dictionary<string, object> InputData { get; private set; } = new();
    public Dictionary<string, decimal> Predictions { get; private set; } = new();
    public decimal ConfidenceScore { get; private set; }
    public string ModelVersion { get; private set; } = string.Empty;

    private PredictiveAnalytics() { } // For EF Core

    public PredictiveAnalytics(string modelName, PredictionType type, DateTime targetDate)
    {
        ModelName = modelName;
        Type = type;
        PredictionDate = DateTime.UtcNow;
        TargetDate = targetDate;
    }

    public void AddPrediction(string metric, decimal value, decimal confidence)
    {
        Predictions[metric] = value;
        ConfidenceScore = confidence;
        SetUpdatedAt();
    }
}

public enum ReportType
{
    Sales,
    Inventory,
    Customer,
    Financial,
    Performance,
    Predictive,
    Compliance
}

public enum ReportStatus
{
    Generating,
    Completed,
    Failed,
    Scheduled
}

public enum CustomerSegment
{
    NewCustomer,
    RegularCustomer,
    VIPCustomer,
    AtRiskCustomer,
    LostCustomer,
    HighValueCustomer
}

public enum PredictionType
{
    SalesForecast,
    DemandForecast,
    ChurnPrediction,
    InventoryOptimization,
    PriceElasticity,
    SeasonalTrends
}