using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Promotions.Commands.CreatePromotion;

public class CreatePromotionCommandHandler : ICommandHandler<CreatePromotionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePromotionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePromotionCommand request, CancellationToken cancellationToken)
    {
        // Verify creator exists
        var creator = await _unitOfWork.Users.GetByIdAsync(request.CreatedByUserId, cancellationToken);
        if (creator == null)
        {
            throw new InvalidOperationException($"User with ID {request.CreatedByUserId} not found");
        }

        // Validate coupon code uniqueness if provided
        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            var existingPromotion = await _unitOfWork.Promotions.GetByCouponCodeAsync(request.CouponCode, cancellationToken);
            if (existingPromotion != null)
            {
                throw new InvalidOperationException($"Coupon code '{request.CouponCode}' is already in use");
            }
        }

        // Create promotion
        var promotion = new Promotion(
            request.Name,
            request.Description,
            request.Type,
            request.DiscountType,
            request.DiscountValue,
            request.StartDate,
            request.EndDate,
            request.CreatedByUserId,
            request.Target);

        // Set optional properties
        if (request.MinimumPurchaseAmount.HasValue || request.MaximumDiscountAmount.HasValue)
        {
            promotion.UpdateDiscountSettings(
                request.DiscountType,
                request.DiscountValue,
                request.MinimumPurchaseAmount,
                request.MaximumDiscountAmount);
        }

        if (request.UsageLimit.HasValue)
        {
            promotion.SetUsageLimit(request.UsageLimit.Value);
        }

        if (!string.IsNullOrEmpty(request.CouponCode))
        {
            promotion.SetCouponCode(request.CouponCode);
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            promotion.UpdateDetails(request.Name, request.Description, request.Notes);
        }

        // Add target products if specified
        if (request.TargetProductIds.Any())
        {
            foreach (var productId in request.TargetProductIds)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
                if (product == null)
                {
                    throw new InvalidOperationException($"Product with ID {productId} not found");
                }
                promotion.AddTargetProduct(productId);
            }
        }

        // Add target categories if specified
        if (request.TargetCategoryIds.Any())
        {
            foreach (var categoryId in request.TargetCategoryIds)
            {
                var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken);
                if (category == null)
                {
                    throw new InvalidOperationException($"Category with ID {categoryId} not found");
                }
                promotion.AddTargetCategory(categoryId);
            }
        }

        await _unitOfWork.Promotions.AddAsync(promotion, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return promotion.Id;
    }
}