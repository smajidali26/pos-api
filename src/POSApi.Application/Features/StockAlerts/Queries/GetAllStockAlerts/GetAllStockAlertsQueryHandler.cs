using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockAlerts.Queries.GetAllStockAlerts;

public class GetAllStockAlertsQueryHandler : IQueryHandler<GetAllStockAlertsQuery, IEnumerable<StockAlertDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllStockAlertsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StockAlertDto>> Handle(GetAllStockAlertsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.StockAlerts.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(a => a.ProductId == request.ProductId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(a => a.LocationId == request.LocationId.Value);

        if (request.AlertType.HasValue)
            query = query.Where(a => a.AlertType == request.AlertType.Value);

        if (request.Severity.HasValue)
            query = query.Where(a => a.Severity == request.Severity.Value);

        if (request.Status.HasValue)
            query = query.Where(a => a.Status == request.Status.Value);

        if (request.IsOverdue.HasValue)
            query = query.Where(a => a.IsOverdue == request.IsOverdue.Value);

        if (request.StartDate.HasValue)
            query = query.Where(a => a.TriggeredDate >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(a => a.TriggeredDate <= request.EndDate.Value);

        var alerts = await query
            .Include(a => a.Product)
            .Include(a => a.Location)
            .Include(a => a.AcknowledgedBy)
            .Include(a => a.ResolvedBy)
            .OrderByDescending(a => a.Severity)
            .ThenByDescending(a => a.TriggeredDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<StockAlertDto>>(alerts);
    }
}
