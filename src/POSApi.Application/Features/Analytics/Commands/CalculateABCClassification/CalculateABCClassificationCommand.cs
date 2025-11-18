using FluentValidation;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.Services;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Analytics.Commands.CalculateABCClassification;

public class CalculateABCClassificationCommand : ICommand<bool>
{
    public Guid? StoreId { get; set; }
    public int PeriodDays { get; set; } = 365;
}

public class CalculateABCClassificationCommandValidator : AbstractValidator<CalculateABCClassificationCommand>
{
    public CalculateABCClassificationCommandValidator()
    {
        RuleFor(x => x.PeriodDays)
            .GreaterThan(0).WithMessage("Period days must be greater than 0")
            .LessThanOrEqualTo(730).WithMessage("Period days cannot exceed 730 (2 years)");
    }
}

public class CalculateABCClassificationCommandHandler : ICommandHandler<CalculateABCClassificationCommand, bool>
{
    private readonly IABCAnalysisService _abcService;
    private readonly IUnitOfWork _unitOfWork;

    public CalculateABCClassificationCommandHandler(IABCAnalysisService abcService, IUnitOfWork unitOfWork)
    {
        _abcService = abcService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CalculateABCClassificationCommand request, CancellationToken cancellationToken)
    {
        // Calculate ABC classification
        var analysis = await _abcService.CalculateABCClassification(request.StoreId, request.PeriodDays, cancellationToken);

        // Delete existing classifications for this store
        var existingClassifications = await _unitOfWork.Context.Set<ProductABCClassification>()
            .Where(pac => pac.StoreId == request.StoreId)
            .ToListAsync(cancellationToken);

        _unitOfWork.Context.Set<ProductABCClassification>().RemoveRange(existingClassifications);

        // Save new classifications
        var allProducts = new List<DTOs.Analytics.ABCClassificationItemDto>();
        allProducts.AddRange(analysis.ClassAProducts);
        allProducts.AddRange(analysis.ClassBProducts);
        allProducts.AddRange(analysis.ClassCProducts);

        foreach (var item in allProducts)
        {
            var abcClass = Enum.Parse<ABCClass>(item.Classification);

            var classification = new ProductABCClassification(
                item.ProductId,
                request.StoreId,
                abcClass,
                item.AnnualVolume,
                item.AnnualRevenue,
                item.ContributionPercentage,
                item.Rank,
                item.CumulativePercentage
            );

            _unitOfWork.Context.Set<ProductABCClassification>().Add(classification);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
