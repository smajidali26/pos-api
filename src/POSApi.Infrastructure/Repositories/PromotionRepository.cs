using Microsoft.EntityFrameworkCore;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories.Interfaces;

namespace POSApi.Infrastructure.Repositories;

public class PromotionRepository : Repository<Promotion>, IPromotionRepository
{
    public PromotionRepository(PosDbContext context) : base(context)
    {
    }

    public async Task<Promotion?> GetByCouponCodeAsync(string couponCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .FirstOrDefaultAsync(p => p.CouponCode == couponCode && p.IsActive, cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync(CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && 
                       p.StartDate <= currentDate && 
                       p.EndDate >= currentDate &&
                       (!p.UsageLimit.HasValue || p.UsageCount < p.UsageLimit.Value))
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsByTypeAsync(PromotionType type, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.Type == type)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsForProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && 
                       p.StartDate <= currentDate && 
                       p.EndDate >= currentDate &&
                       (p.Target == PromotionTarget.Order ||
                        p.PromotionProducts.Any(pp => pp.ProductId == productId)))
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsForCategoryAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive && 
                       p.StartDate <= currentDate && 
                       p.EndDate >= currentDate &&
                       (p.Target == PromotionTarget.Order ||
                        p.PromotionCategories.Any(pc => pc.CategoryId == categoryId)))
            .OrderBy(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionUsages)
            .Where(p => p.StartDate <= endDate && p.EndDate >= startDate)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetExpiredPromotionsAsync(CancellationToken cancellationToken = default)
    {
        var currentDate = DateTime.UtcNow;
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Where(p => p.EndDate < currentDate)
            .OrderByDescending(p => p.EndDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsCreatedByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionUsages)
            .Where(p => p.CreatedByUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalDiscountForPeriodAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.Set<PromotionUsage>()
            .Where(pu => pu.UsedAt >= startDate && pu.UsedAt <= endDate)
            .SumAsync(pu => pu.DiscountAmount, cancellationToken);
    }

    public override async Task<Promotion?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.PromotionUsages)
                .ThenInclude(pu => pu.Order)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<Promotion>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.CreatedBy)
            .Include(p => p.PromotionProducts)
                .ThenInclude(pp => pp.Product)
            .Include(p => p.PromotionCategories)
                .ThenInclude(pc => pc.Category)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}