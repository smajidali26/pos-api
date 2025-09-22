using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.PurchaseOrders.Queries.GetPendingReceiptOrders;

public class GetPendingReceiptOrdersQuery : IQuery<IEnumerable<PurchaseOrderDto>>
{
}

public class GetPendingReceiptOrdersQueryHandler : IQueryHandler<GetPendingReceiptOrdersQuery, IEnumerable<PurchaseOrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPendingReceiptOrdersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> Handle(GetPendingReceiptOrdersQuery request, CancellationToken cancellationToken)
    {
        var pendingOrders = await _unitOfWork.PurchaseOrders.GetPendingReceiptAsync(cancellationToken);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(pendingOrders);
    }
}