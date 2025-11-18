using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Queries.GetBatchById;

public class GetBatchByIdQueryHandler : IQueryHandler<GetBatchByIdQuery, BatchDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetBatchByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BatchDto> Handle(GetBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var batch = await _unitOfWork.Context.Batches
            .Include(b => b.Product)
            .Include(b => b.Vendor)
            .Include(b => b.Location)
            .Include(b => b.RecalledBy)
            .FirstOrDefaultAsync(b => b.Id == request.BatchId, cancellationToken);

        if (batch == null)
            throw new InvalidOperationException($"Batch with ID {request.BatchId} not found");

        return _mapper.Map<BatchDto>(batch);
    }
}
