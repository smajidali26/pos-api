using FluentValidation;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.Services;

namespace POSApi.Application.Features.Analytics.Queries.GetInventoryTurnover;

public class GetInventoryTurnoverQuery : IQuery<InventoryTurnoverSummaryDto>
{
    public Guid? StoreId { get; set; }
    public Guid? CategoryId { get; set; }
    public int PeriodDays { get; set; } = 365;
}

public class GetInventoryTurnoverQueryValidator : AbstractValidator<GetInventoryTurnoverQuery>
{
    public GetInventoryTurnoverQueryValidator()
    {
        RuleFor(x => x.PeriodDays)
            .GreaterThan(0).WithMessage("Period days must be greater than 0")
            .LessThanOrEqualTo(730).WithMessage("Period days cannot exceed 730 (2 years)");
    }
}

public class GetInventoryTurnoverQueryHandler : IQueryHandler<GetInventoryTurnoverQuery, InventoryTurnoverSummaryDto>
{
    private readonly IInventoryTurnoverService _turnoverService;

    public GetInventoryTurnoverQueryHandler(IInventoryTurnoverService turnoverService)
    {
        _turnoverService = turnoverService;
    }

    public async Task<InventoryTurnoverSummaryDto> Handle(GetInventoryTurnoverQuery request, CancellationToken cancellationToken)
    {
        return await _turnoverService.CalculateTurnoverRatio(
            request.StoreId, request.CategoryId, request.PeriodDays, cancellationToken);
    }
}
