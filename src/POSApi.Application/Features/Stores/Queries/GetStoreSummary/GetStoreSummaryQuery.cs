using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Stores.Queries.GetStoreSummary;

public class GetStoreSummaryQuery : IQuery<StoreSummaryDto?>
{
    public Guid StoreId { get; set; }

    public GetStoreSummaryQuery(Guid storeId)
    {
        StoreId = storeId;
    }
}

public class GetStoreSummaryQueryHandler : IQueryHandler<GetStoreSummaryQuery, StoreSummaryDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStoreSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StoreSummaryDto?> Handle(GetStoreSummaryQuery request, CancellationToken cancellationToken)
    {
        var store = await _unitOfWork.Stores.GetByIdAsync(request.StoreId, cancellationToken);
        if (store == null)
        {
            return null;
        }

        var summary = await _unitOfWork.Stores.GetStoreSummaryAsync(request.StoreId, cancellationToken);
        return summary;
    }
}
