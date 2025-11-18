using POSApi.Application.Common.DTOs.Analytics;

namespace POSApi.Application.Common.Services;

public interface IInventoryTurnoverService
{
    /// <summary>
    /// Calculate inventory turnover ratio for products
    /// </summary>
    Task<InventoryTurnoverSummaryDto> CalculateTurnoverRatio(Guid? storeId = null, Guid? categoryId = null, int periodDays = 365, CancellationToken cancellationToken = default);

    /// <summary>
    /// Identify slow-moving stock items
    /// </summary>
    Task<List<InventoryTurnoverDto>> IdentifySlowMovingStock(Guid? storeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate reorder recommendations based on turnover and stock levels
    /// </summary>
    Task<List<ReorderRecommendationDto>> GenerateReorderRecommendations(Guid? storeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Classify turnover rate
    /// </summary>
    string ClassifyTurnoverRate(decimal turnoverRatio);
}
