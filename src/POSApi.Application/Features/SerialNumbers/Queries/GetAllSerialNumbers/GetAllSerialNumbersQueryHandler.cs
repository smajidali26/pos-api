using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Queries.GetAllSerialNumbers;

public class GetAllSerialNumbersQueryHandler : IQueryHandler<GetAllSerialNumbersQuery, IEnumerable<SerialNumberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllSerialNumbersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SerialNumberDto>> Handle(GetAllSerialNumbersQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Context.SerialNumbers.AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(sn => sn.ProductId == request.ProductId.Value);

        if (request.BatchId.HasValue)
            query = query.Where(sn => sn.BatchId == request.BatchId.Value);

        if (request.LocationId.HasValue)
            query = query.Where(sn => sn.LocationId == request.LocationId.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(sn => sn.CustomerId == request.CustomerId.Value);

        if (request.Status.HasValue)
            query = query.Where(sn => sn.Status == request.Status.Value);

        if (request.IsUnderWarranty.HasValue)
            query = query.Where(sn => sn.IsUnderWarranty == request.IsUnderWarranty.Value);

        if (request.StartDate.HasValue)
            query = query.Where(sn => sn.CreatedAt >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(sn => sn.CreatedAt <= request.EndDate.Value);

        var serialNumbers = await query
            .Include(sn => sn.Product)
            .Include(sn => sn.Batch)
            .Include(sn => sn.Location)
            .Include(sn => sn.Customer)
            .Include(sn => sn.Order)
            .Include(sn => sn.History)
            .OrderByDescending(sn => sn.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<SerialNumberDto>>(serialNumbers);
    }
}
