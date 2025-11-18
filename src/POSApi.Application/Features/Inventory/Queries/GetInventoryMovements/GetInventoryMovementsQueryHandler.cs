using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Inventory.Queries.GetInventoryMovements;

public class GetInventoryMovementsQueryHandler : IQueryHandler<GetInventoryMovementsQuery, IEnumerable<InventoryMovementDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetInventoryMovementsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<InventoryMovementDto>> Handle(GetInventoryMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.InventoryMovements.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(m => m.ProductId == request.ProductId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(m => m.LocationId == request.LocationId.Value);

        if (request.MovementType.HasValue)
            query = query.Where(m => m.Type == request.MovementType.Value);

        if (request.StartDate.HasValue)
            query = query.Where(m => m.MovementDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(m => m.MovementDate <= request.EndDate.Value);

        var movements = await query
            .Include(m => m.Product)
            .Include(m => m.Location)
            .Include(m => m.MovedBy)
            .OrderByDescending(m => m.MovementDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<InventoryMovementDto>>(movements);
    }
}
