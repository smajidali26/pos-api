using POSApi.Application.Common.DTOs.Analytics;

namespace POSApi.Application.Common.Services;

public interface IABCAnalysisService
{
    /// <summary>
    /// Calculate ABC classification for all products
    /// </summary>
    Task<ABCAnalysisDto> CalculateABCClassification(Guid? storeId = null, int periodDays = 365, CancellationToken cancellationToken = default);

    /// <summary>
    /// Classify a single product based on its contribution
    /// </summary>
    string ClassifyProduct(decimal contributionPercentage, decimal cumulativePercentage);

    /// <summary>
    /// Generate recommended strategy for each ABC class
    /// </summary>
    string GetRecommendedStrategy(string classification);
}
