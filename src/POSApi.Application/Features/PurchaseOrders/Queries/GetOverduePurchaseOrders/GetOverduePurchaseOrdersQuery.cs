using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetOverduePurchaseOrders;

public class GetOverduePurchaseOrdersQuery : IQuery<IEnumerable<PurchaseOrderDto>>
{
}

public class GetOverduePurchaseOrdersQueryHandler : IQueryHandler<GetOverduePurchaseOrdersQuery, IEnumerable<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetOverduePurchaseOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> Handle(GetOverduePurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var overduePurchaseOrders = await _unitOfWork.PurchaseOrders.GetOverduePurchaseOrdersAsync(cancellationToken);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(overduePurchaseOrders);
    }
}