using FluentValidation;
using POSApi.Application.Common.DTOs.Analytics;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.Services;

namespace POSApi.Application.Features.Analytics.Queries.GetABCAnalysis;

public class GetABCAnalysisQuery : IQuery<ABCAnalysisDto>
{
    public Guid? StoreId { get; set; }
    public int PeriodDays { get; set; } = 365;
}

public class GetABCAnalysisQueryValidator : AbstractValidator<GetABCAnalysisQuery>
{
    public GetABCAnalysisQueryValidator()
    {
        RuleFor(x => x.PeriodDays)
            .GreaterThan(0).WithMessage("Period days must be greater than 0")
            .LessThanOrEqualTo(730).WithMessage("Period days cannot exceed 730 (2 years)");
    }
}

public class GetABCAnalysisQueryHandler : IQueryHandler<GetABCAnalysisQuery, ABCAnalysisDto>
{
    private readonly IABCAnalysisService _abcService;

    public GetABCAnalysisQueryHandler(IABCAnalysisService abcService)
    {
        _abcService = abcService;
    }

    public async Task<ABCAnalysisDto> Handle(GetABCAnalysisQuery request, CancellationToken cancellationToken)
    {
        return await _abcService.CalculateABCClassification(request.StoreId, request.PeriodDays, cancellationToken);
    }
}
