using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetPurchaseOrderById;

public class GetPurchaseOrderByIdQuery : IQuery<PurchaseOrderDto?>
{
    public Guid Id { get; set; }

    public GetPurchaseOrderByIdQuery(Guid id)
    {
        Id = id;
    }
}

public class GetPurchaseOrderByIdQueryHandler : IQueryHandler<GetPurchaseOrderByIdQuery, PurchaseOrderDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPurchaseOrderByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PurchaseOrderDto?> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrder = await _unitOfWork.PurchaseOrders.GetByIdAsync(request.Id, cancellationToken);
        return purchaseOrder == null ? null : _mapper.Map<PurchaseOrderDto>(purchaseOrder);
    }
}