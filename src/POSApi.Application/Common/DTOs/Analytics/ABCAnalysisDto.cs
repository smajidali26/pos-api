namespace POSApi.Application.Common.DTOs.Analytics;

public class ABCAnalysisDto
{
    public ABCAnalysisSummaryDto Summary { get; set; } = new();
    public List<ABCClassificationItemDto> ClassAProducts { get; set; } = new();
    public List<ABCClassificationItemDto> ClassBProducts { get; set; } = new();
    public List<ABCClassificationItemDto> ClassCProducts { get; set; } = new();
    public List<CategoryABCDto> ByCategory { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

public class ABCAnalysisSummaryDto
{
    public int TotalProducts { get; set; }
    public int ClassACount { get; set; }
    public int ClassBCount { get; set; }
    public int ClassCCount { get; set; }
    public decimal ClassARevenue { get; set; }
    public decimal ClassBRevenue { get; set; }
    public decimal ClassCRevenue { get; set; }
    public decimal ClassAPercentage { get; set; }
    public decimal ClassBPercentage { get; set; }
    public decimal ClassCPercentage { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class ABCClassificationItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public string Classification { get; set; } = string.Empty;
    public decimal AnnualVolume { get; set; }
    public decimal AnnualRevenue { get; set; }
    public decimal ContributionPercentage { get; set; }
    public decimal CumulativePercentage { get; set; }
    public int Rank { get; set; }
    public DateTime LastCalculatedAt { get; set; }
    public string RecommendedStrategy { get; set; } = string.Empty;
}

public class CategoryABCDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ClassACount { get; set; }
    public int ClassBCount { get; set; }
    public int ClassCCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
