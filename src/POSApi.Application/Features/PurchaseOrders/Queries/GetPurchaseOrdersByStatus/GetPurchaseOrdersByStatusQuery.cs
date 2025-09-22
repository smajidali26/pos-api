using AutoMapper;
using FluentValidation;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrdersByStatus;

public class GetPurchaseOrdersByStatusQuery : IQuery<IEnumerable<PurchaseOrderDto>>
{
    public PurchaseOrderStatus Status { get; set; }

    public GetPurchaseOrdersByStatusQuery(PurchaseOrderStatus status)
    {
        Status = status;
    }
}

public class GetPurchaseOrdersByStatusQueryValidator : AbstractValidator<GetPurchaseOrdersByStatusQuery>
{
    public GetPurchaseOrdersByStatusQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid purchase order status");
    }
}

public class GetPurchaseOrdersByStatusQueryHandler : IQueryHandler<GetPurchaseOrdersByStatusQuery, IEnumerable<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPurchaseOrdersByStatusQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> Handle(GetPurchaseOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrders = await _unitOfWork.PurchaseOrders.GetByStatusAsync(request.Status, cancellationToken);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
    }
}