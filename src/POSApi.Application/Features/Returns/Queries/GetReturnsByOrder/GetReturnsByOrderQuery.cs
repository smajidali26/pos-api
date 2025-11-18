using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Returns.Queries.GetReturnsByOrder;

public class GetReturnsByOrderQuery : IQuery<IEnumerable<ReturnDto>>
{
    public Guid OrderId { get; set; }

    public GetReturnsByOrderQuery(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class GetReturnsByOrderQueryHandler : IQueryHandler<GetReturnsByOrderQuery, IEnumerable<ReturnDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetReturnsByOrderQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ReturnDto>> Handle(GetReturnsByOrderQuery request, CancellationToken cancellationToken)
    {
        var returns = await _unitOfWork.Returns.GetByOriginalOrderIdAsync(request.OrderId, cancellationToken);
        return _mapper.Map<IEnumerable<ReturnDto>>(returns);
    }
}
