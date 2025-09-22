using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Promotions.Commands.CalculateDiscount;

public class CalculateDiscountCommandHandler : ICommandHandler<CalculateDiscountCommand, DiscountCalculationResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public CalculateDiscountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<DiscountCalculationResult> Handle(CalculateDiscountCommand request, CancellationToken cancellationToken)
    {
        var result = new DiscountCalculationResult
        {
            OriginalAmount = request.OrderAmount,
            FinalAmount = request.OrderAmount
        };

        var applicablePromotions = new List<Promotion>();

        try
        {
            // Get coupon-based promotion if provided
            if (!string.IsNullOrEmpty(request.CouponCode))
            {
                var couponPromotion = await _unitOfWork.Promotions.GetByCouponCodeAsync(request.CouponCode, cancellationToken);
                if (couponPromotion != null && couponPromotion.IsValidForUse(DateTime.UtcNow))
                {
                    applicablePromotions.Add(couponPromotion);
                }
                else
                {
                    result.HasErrors = true;
                    result.ErrorMessages.Add("Invalid or expired coupon code");
                    return result;
                }
            }

            // Get automatic promotions if enabled
            if (request.ApplyAutomaticPromotions)
            {
                var automaticPromotions = await GetApplicableAutomaticPromotions(request, cancellationToken);
                applicablePromotions.AddRange(automaticPromotions);
            }

            // Calculate discounts for each applicable promotion
            decimal totalDiscount = 0;
            var productIds = request.OrderItems.Select(oi => oi.ProductId).ToList();

            foreach (var promotion in applicablePromotions)
            {
                var discount = await CalculatePromotionDiscount(promotion, request, productIds, cancellationToken);
                
                if (discount > 0)
                {
                    totalDiscount += discount;
                    result.AppliedPromotions.Add(new AppliedPromotion
                    {
                        PromotionId = promotion.Id,
                        PromotionName = promotion.Name,
                        PromotionType = promotion.Type.ToString(),
                        DiscountAmount = discount,
                        Description = GetPromotionDescription(promotion, discount)
                    });
                }

                // Handle non-stackable promotions
                if (!promotion.IsStackable && discount > 0)
                {
                    break;
                }
            }

            result.TotalDiscount = totalDiscount;
            result.FinalAmount = Math.Max(0, request.OrderAmount - totalDiscount);
        }
        catch (Exception ex)
        {
            result.HasErrors = true;
            result.ErrorMessages.Add($"Error calculating discounts: {ex.Message}");
        }

        return result;
    }

    private async Task<List<Promotion>> GetApplicableAutomaticPromotions(CalculateDiscountCommand request, CancellationToken cancellationToken)
    {
        var activePromotions = await _unitOfWork.Promotions.GetActivePromotionsAsync(cancellationToken);
        var applicablePromotions = new List<Promotion>();

        foreach (var promotion in activePromotions)
        {
            // Skip coupon-based promotions for automatic application
            if (promotion.Type == PromotionType.Coupon)
                continue;

            // Check minimum purchase amount
            if (promotion.MinimumPurchaseAmount.HasValue && request.OrderAmount < promotion.MinimumPurchaseAmount.Value)
                continue;

            // Check if promotion applies to products in the order
            if (await IsPromotionApplicableToOrder(promotion, request, cancellationToken))
            {
                applicablePromotions.Add(promotion);
            }
        }

        // Sort by discount amount (highest first) for non-stackable scenarios
        return applicablePromotions.OrderByDescending(p => p.CalculateDiscount(request.OrderAmount, request.OrderItems.Sum(oi => oi.Quantity))).ToList();
    }

    private async Task<bool> IsPromotionApplicableToOrder(Promotion promotion, CalculateDiscountCommand request, CancellationToken cancellationToken)
    {
        switch (promotion.Target)
        {
            case PromotionTarget.Order:
                return true;

            case PromotionTarget.Product:
            case PromotionTarget.ProductGroup:
                var promotionProductIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();
                return request.OrderItems.Any(oi => promotionProductIds.Contains(oi.ProductId));

            case PromotionTarget.Category:
                var promotionCategoryIds = promotion.PromotionCategories.Select(pc => pc.CategoryId).ToList();
                foreach (var orderItem in request.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId, cancellationToken);
                    if (product != null && promotionCategoryIds.Contains(product.CategoryId))
                    {
                        return true;
                    }
                }
                return false;

            default:
                return false;
        }
    }

    private async Task<decimal> CalculatePromotionDiscount(Promotion promotion, CalculateDiscountCommand request, List<Guid> productIds, CancellationToken cancellationToken)
    {
        decimal applicableAmount = request.OrderAmount;
        int applicableQuantity = request.OrderItems.Sum(oi => oi.Quantity);

        // For product or category-specific promotions, calculate applicable amount
        if (promotion.Target == PromotionTarget.Product || promotion.Target == PromotionTarget.ProductGroup)
        {
            var promotionProductIds = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();
            applicableAmount = request.OrderItems
                .Where(oi => promotionProductIds.Contains(oi.ProductId))
                .Sum(oi => oi.TotalPrice);
            applicableQuantity = request.OrderItems
                .Where(oi => promotionProductIds.Contains(oi.ProductId))
                .Sum(oi => oi.Quantity);
        }
        else if (promotion.Target == PromotionTarget.Category)
        {
            applicableAmount = 0;
            applicableQuantity = 0;
            var promotionCategoryIds = promotion.PromotionCategories.Select(pc => pc.CategoryId).ToList();
            
            foreach (var orderItem in request.OrderItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(orderItem.ProductId, cancellationToken);
                if (product != null && promotionCategoryIds.Contains(product.CategoryId))
                {
                    applicableAmount += orderItem.TotalPrice;
                    applicableQuantity += orderItem.Quantity;
                }
            }
        }

        return promotion.CalculateDiscount(applicableAmount, applicableQuantity);
    }

    private static string GetPromotionDescription(Promotion promotion, decimal discountAmount)
    {
        return promotion.DiscountType switch
        {
            DiscountType.Percentage => $"{promotion.DiscountValue}% off - ${discountAmount:F2}",
            DiscountType.FixedAmount => $"${promotion.DiscountValue:F2} off",
            DiscountType.BuyXGetYFree => $"Buy {promotion.DiscountValue} get 1 free - ${discountAmount:F2}",
            _ => $"${discountAmount:F2} discount"
        };
    }
}