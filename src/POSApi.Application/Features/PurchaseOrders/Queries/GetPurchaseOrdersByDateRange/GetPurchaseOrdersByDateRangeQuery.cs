using AutoMapper;
using FluentValidation;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrdersByDateRange;

public class GetPurchaseOrdersByDateRangeQuery : IQuery<IEnumerable<PurchaseOrderDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public GetPurchaseOrdersByDateRangeQuery(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }
}

public class GetPurchaseOrdersByDateRangeQueryValidator : AbstractValidator<GetPurchaseOrdersByDateRangeQuery>
{
    public GetPurchaseOrdersByDateRangeQueryValidator()
    {
        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date must be greater than or equal to start date");
    }
}

public class GetPurchaseOrdersByDateRangeQueryHandler : IQueryHandler<GetPurchaseOrdersByDateRangeQuery, IEnumerable<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPurchaseOrdersByDateRangeQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> Handle(GetPurchaseOrdersByDateRangeQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrders = await _unitOfWork.PurchaseOrders.GetByDateRangeAsync(request.StartDate, request.EndDate, cancellationToken);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
    }
}