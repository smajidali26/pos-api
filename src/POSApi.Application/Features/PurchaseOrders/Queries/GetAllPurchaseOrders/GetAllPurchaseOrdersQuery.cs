using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetAllPurchaseOrders;

public class GetAllPurchaseOrdersQuery : IQuery<IEnumerable<PurchaseOrderDto>>
{
}

public class GetAllPurchaseOrdersQueryHandler : IQueryHandler<GetAllPurchaseOrdersQuery, IEnumerable<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllPurchaseOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> Handle(GetAllPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var purchaseOrders = await _unitOfWork.PurchaseOrders.GetAllAsync(cancellationToken);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(purchaseOrders);
    }
}