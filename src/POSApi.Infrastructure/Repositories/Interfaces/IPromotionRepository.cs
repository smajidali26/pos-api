using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Infrastructure.Repositories.Interfaces;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<Promotion?> GetByCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPromotionsByTypeAsync(PromotionType type, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPromotionsForProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPromotionsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPromotionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetExpiredPromotionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Promotion>> GetPromotionsCreatedByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalDiscountForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}