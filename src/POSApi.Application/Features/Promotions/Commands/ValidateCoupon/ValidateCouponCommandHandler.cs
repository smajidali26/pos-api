using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Promotions.Commands.ValidateCoupon;

public class ValidateCouponCommandHandler : ICommandHandler<ValidateCouponCommand, CouponValidationResult>
{
    private readonly IUnitOfWork _unitOfWork;

    public ValidateCouponCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CouponValidationResult> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        var result = new CouponValidationResult();

        // Find promotion by coupon code
        var promotion = await _unitOfWork.Promotions.GetByCouponCodeAsync(request.CouponCode, cancellationToken);
        
        if (promotion == null)
        {
            result.IsValid = false;
            result.Message = "Invalid coupon code";
            return result;
        }

        // Check if promotion is currently active
        if (!promotion.IsValidForUse(DateTime.UtcNow))
        {
            result.IsValid = false;
            result.Message = GetInactiveReason(promotion);
            result.PromotionId = promotion.Id;
            result.PromotionName = promotion.Name;
            result.ExpirationDate = promotion.EndDate;
            return result;
        }

        // Check minimum purchase requirement
        if (promotion.MinimumPurchaseAmount.HasValue && request.OrderAmount < promotion.MinimumPurchaseAmount.Value)
        {
            result.IsValid = false;
            result.Message = $"Minimum purchase amount of ${promotion.MinimumPurchaseAmount:F2} required";
            result.PromotionId = promotion.Id;
            result.PromotionName = promotion.Name;
            return result;
        }

        // Check if promotion applies to the products in the order
        if (promotion.Target == PromotionTarget.Product || promotion.Target == PromotionTarget.ProductGroup)
        {
            var applicableProducts = promotion.PromotionProducts.Select(pp => pp.ProductId).ToList();
            var hasApplicableProducts = request.ProductIds.Any(pid => applicableProducts.Contains(pid));
            
            if (!hasApplicableProducts)
            {
                result.IsValid = false;
                result.Message = "This coupon is not applicable to the products in your order";
                result.PromotionId = promotion.Id;
                result.PromotionName = promotion.Name;
                return result;
            }
        }

        // Check if promotion applies to product categories
        if (promotion.Target == PromotionTarget.Category && request.ProductIds.Any())
        {
            var applicableCategories = promotion.PromotionCategories.Select(pc => pc.CategoryId).ToList();
            var hasApplicableCategories = false;

            foreach (var productId in request.ProductIds)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
                if (product != null && applicableCategories.Contains(product.CategoryId))
                {
                    hasApplicableCategories = true;
                    break;
                }
            }

            if (!hasApplicableCategories)
            {
                result.IsValid = false;
                result.Message = "This coupon is not applicable to the product categories in your order";
                result.PromotionId = promotion.Id;
                result.PromotionName = promotion.Name;
                return result;
            }
        }

        // Calculate potential discount
        var potentialDiscount = promotion.CalculateDiscount(request.OrderAmount, request.ProductIds.Count);

        result.IsValid = true;
        result.Message = $"Coupon is valid! You'll save ${potentialDiscount:F2}";
        result.PromotionId = promotion.Id;
        result.PromotionName = promotion.Name;
        result.PotentialDiscount = potentialDiscount;
        result.ExpirationDate = promotion.EndDate;

        return result;
    }

    private static string GetInactiveReason(Promotion promotion)
    {
        var now = DateTime.UtcNow;
        
        if (!promotion.IsActive)
            return "This coupon is no longer active";
        
        if (now < promotion.StartDate)
            return $"This coupon is not yet active. Valid from {promotion.StartDate:MMM dd, yyyy}";
        
        if (now > promotion.EndDate)
            return $"This coupon has expired on {promotion.EndDate:MMM dd, yyyy}";
        
        if (promotion.UsageLimit.HasValue && promotion.UsageCount >= promotion.UsageLimit.Value)
            return "This coupon has reached its usage limit";
        
        return "This coupon is currently not available";
    }
}