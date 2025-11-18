using AutoMapper;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Queries.GetStoreInventory;

public class GetStoreInventoryQuery : IQuery<List<StoreInventoryDto>>
{
    public Guid StoreId { get; set; }
    public bool? LowStockOnly { get; set; }
    public bool? OutOfStockOnly { get; set; }
}

public class GetStoreInventoryQueryHandler : IQueryHandler<GetStoreInventoryQuery, List<StoreInventoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetStoreInventoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<StoreInventoryDto>> Handle(GetStoreInventoryQuery request, CancellationToken cancellationToken)
    {
        var inventory = await _unitOfWork.StoreInventories.GetByStoreIdAsync(
            request.StoreId,
            request.LowStockOnly,
            request.OutOfStockOnly,
            cancellationToken);

        return _mapper.Map<List<StoreInventoryDto>>(inventory);
    }
}
