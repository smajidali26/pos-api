using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Promotions.Queries.GetPromotionAnalytics;

public class GetPromotionAnalyticsQuery : IQuery<PromotionAnalyticsDto>
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetPromotionAnalyticsQueryHandler : IQueryHandler<GetPromotionAnalyticsQuery, PromotionAnalyticsDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPromotionAnalyticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PromotionAnalyticsDto> Handle(GetPromotionAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startDate = request.StartDate ?? DateTime.MinValue;
        var endDate = request.EndDate ?? DateTime.MaxValue;

        // Get all promotions within the date range (or all if no dates specified)
        var promotions = await _unitOfWork.Promotions.GetAllAsync(cancellationToken);

        // Filter by date range if provided
        var filteredPromotions = promotions;
        if (request.StartDate.HasValue || request.EndDate.HasValue)
        {
            filteredPromotions = promotions.Where(p =>
                p.StartDate >= startDate && p.StartDate <= endDate ||
                p.EndDate >= startDate && p.EndDate <= endDate ||
                p.StartDate <= startDate && p.EndDate >= endDate
            ).ToList();
        }

        var totalPromotions = filteredPromotions.Count();
        var activePromotions = filteredPromotions.Count(p => p.IsActive && p.StartDate <= now && p.EndDate >= now);
        var inactivePromotions = filteredPromotions.Count(p => !p.IsActive);
        var expiredPromotions = filteredPromotions.Count(p => p.EndDate < now);

        // Calculate total discount given and usage count across all promotions
        decimal totalDiscountGiven = 0;
        int totalUsageCount = 0;

        foreach (var promotion in filteredPromotions)
        {
            var usages = promotion.PromotionUsages;

            // Filter usages by date range if provided
            if (request.StartDate.HasValue || request.EndDate.HasValue)
            {
                usages = usages.Where(u => u.UsedAt >= startDate && u.UsedAt <= endDate).ToList();
            }

            totalDiscountGiven += usages.Sum(u => u.DiscountAmount);
            totalUsageCount += usages.Count();
        }

        var averageDiscountPerPromotion = totalPromotions > 0 ? totalDiscountGiven / totalPromotions : 0;
        var averageUsagePerPromotion = totalPromotions > 0 ? (decimal)totalUsageCount / totalPromotions : 0;

        // Get top promotions by usage count
        var topPromotions = filteredPromotions
            .Select(p => new
            {
                Promotion = p,
                UsageCount = request.StartDate.HasValue || request.EndDate.HasValue
                    ? p.PromotionUsages.Count(u => u.UsedAt >= startDate && u.UsedAt <= endDate)
                    : p.PromotionUsages.Count,
                TotalDiscount = request.StartDate.HasValue || request.EndDate.HasValue
                    ? p.PromotionUsages.Where(u => u.UsedAt >= startDate && u.UsedAt <= endDate).Sum(u => u.DiscountAmount)
                    : p.PromotionUsages.Sum(u => u.DiscountAmount)
            })
            .OrderByDescending(p => p.UsageCount)
            .Take(10)
            .Select(p => new TopPromotionDto
            {
                PromotionId = p.Promotion.Id,
                Name = p.Promotion.Name,
                Description = p.Promotion.Description,
                Type = p.Promotion.Type,
                UsageCount = p.UsageCount,
                TotalDiscountGiven = p.TotalDiscount,
                IsActive = p.Promotion.IsActive,
                StartDate = p.Promotion.StartDate,
                EndDate = p.Promotion.EndDate
            })
            .ToList();

        // Get promotion type breakdown
        var promotionTypeBreakdown = filteredPromotions
            .GroupBy(p => p.Type)
            .Select(g => new
            {
                Type = g.Key,
                Promotions = g.ToList()
            })
            .Select(g => new PromotionTypeBreakdownDto
            {
                Type = g.Type,
                TypeName = g.Type.ToString(),
                Count = g.Promotions.Count,
                UsageCount = request.StartDate.HasValue || request.EndDate.HasValue
                    ? g.Promotions.Sum(p => p.PromotionUsages.Count(u => u.UsedAt >= startDate && u.UsedAt <= endDate))
                    : g.Promotions.Sum(p => p.PromotionUsages.Count),
                TotalDiscountGiven = request.StartDate.HasValue || request.EndDate.HasValue
                    ? g.Promotions.Sum(p => p.PromotionUsages.Where(u => u.UsedAt >= startDate && u.UsedAt <= endDate).Sum(u => u.DiscountAmount))
                    : g.Promotions.Sum(p => p.PromotionUsages.Sum(u => u.DiscountAmount))
            })
            .OrderByDescending(b => b.UsageCount)
            .ToList();

        // Get discount type breakdown
        var discountTypeBreakdown = filteredPromotions
            .GroupBy(p => p.DiscountType)
            .Select(g => new
            {
                Type = g.Key,
                Promotions = g.ToList()
            })
            .Select(g => new DiscountTypeBreakdownDto
            {
                Type = g.Type,
                TypeName = g.Type.ToString(),
                Count = g.Promotions.Count,
                UsageCount = request.StartDate.HasValue || request.EndDate.HasValue
                    ? g.Promotions.Sum(p => p.PromotionUsages.Count(u => u.UsedAt >= startDate && u.UsedAt <= endDate))
                    : g.Promotions.Sum(p => p.PromotionUsages.Count),
                TotalDiscountGiven = request.StartDate.HasValue || request.EndDate.HasValue
                    ? g.Promotions.Sum(p => p.PromotionUsages.Where(u => u.UsedAt >= startDate && u.UsedAt <= endDate).Sum(u => u.DiscountAmount))
                    : g.Promotions.Sum(p => p.PromotionUsages.Sum(u => u.DiscountAmount))
            })
            .OrderByDescending(b => b.UsageCount)
            .ToList();

        return new PromotionAnalyticsDto
        {
            TotalPromotions = totalPromotions,
            ActivePromotions = activePromotions,
            InactivePromotions = inactivePromotions,
            ExpiredPromotions = expiredPromotions,
            TotalDiscountGiven = totalDiscountGiven,
            TotalUsageCount = totalUsageCount,
            AverageDiscountPerPromotion = averageDiscountPerPromotion,
            AverageUsagePerPromotion = averageUsagePerPromotion,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TopPromotions = topPromotions,
            PromotionTypeBreakdown = promotionTypeBreakdown,
            DiscountTypeBreakdown = discountTypeBreakdown
        };
    }
}
