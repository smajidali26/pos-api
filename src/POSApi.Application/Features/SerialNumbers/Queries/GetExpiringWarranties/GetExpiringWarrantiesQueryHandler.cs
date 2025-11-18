using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Queries.GetExpiringWarranties;

public class GetExpiringWarrantiesQueryHandler : IQueryHandler<GetExpiringWarrantiesQuery, IEnumerable<SerialNumberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetExpiringWarrantiesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SerialNumberDto>> Handle(GetExpiringWarrantiesQuery request, CancellationToken cancellationToken)
    {
        var thresholdDate = DateTime.UtcNow.AddDays(request.DaysThreshold);

        var query = _unitOfWork.Context.SerialNumbers
            .Where(sn => sn.WarrantyEndDate.HasValue)
            .Where(sn => sn.WarrantyEndDate.Value <= thresholdDate)
            .Where(sn => sn.WarrantyEndDate.Value > DateTime.UtcNow)
            .Where(sn => sn.Status == SerialNumberStatus.Sold || sn.Status == SerialNumberStatus.InWarranty);

        if (request.ProductId.HasValue)
            query = query.Where(sn => sn.ProductId == request.ProductId.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(sn => sn.CustomerId == request.CustomerId.Value);

        var serialNumbers = await query
            .Include(sn => sn.Product)
            .Include(sn => sn.Batch)
            .Include(sn => sn.Location)
            .Include(sn => sn.Customer)
            .Include(sn => sn.Order)
            .Include(sn => sn.History)
            .OrderBy(sn => sn.WarrantyEndDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<SerialNumberDto>>(serialNumbers);
    }
}
