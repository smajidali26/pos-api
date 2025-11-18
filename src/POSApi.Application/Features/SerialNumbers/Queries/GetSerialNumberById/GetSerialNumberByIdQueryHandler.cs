using AutoMapper;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Queries.GetSerialNumberById;

public class GetSerialNumberByIdQueryHandler : IQueryHandler<GetSerialNumberByIdQuery, SerialNumberDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetSerialNumberByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SerialNumberDto> Handle(GetSerialNumberByIdQuery request, CancellationToken cancellationToken)
    {
        var serialNumber = await _unitOfWork.Context.SerialNumbers
            .Include(sn => sn.Product)
            .Include(sn => sn.Batch)
            .Include(sn => sn.Location)
            .Include(sn => sn.Customer)
            .Include(sn => sn.Order)
            .Include(sn => sn.History)
                .ThenInclude(h => h.PerformedBy)
            .FirstOrDefaultAsync(sn => sn.Id == request.SerialNumberId, cancellationToken);

        if (serialNumber == null)
            throw new InvalidOperationException($"Serial number with ID {request.SerialNumberId} not found");

        return _mapper.Map<SerialNumberDto>(serialNumber);
    }
}
