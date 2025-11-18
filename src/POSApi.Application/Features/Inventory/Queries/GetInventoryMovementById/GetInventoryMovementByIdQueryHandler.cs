using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Inventory.Queries.GetInventoryMovementById;

public class GetInventoryMovementByIdQueryHandler : IQueryHandler<GetInventoryMovementByIdQuery, InventoryMovementDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetInventoryMovementByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<InventoryMovementDto> Handle(GetInventoryMovementByIdQuery request, CancellationToken cancellationToken)
    {
        var movement = await _unitOfWork.Context.InventoryMovements
            .Include(m => m.Product)
            .Include(m => m.Location)
            .Include(m => m.PerformedBy)
            .FirstOrDefaultAsync(m => m.Id == request.MovementId, cancellationToken);

        if (movement == null)
            throw new InvalidOperationException($"Inventory movement with ID {request.MovementId} not found");

        return _mapper.Map<InventoryMovementDto>(movement);
    }
}
