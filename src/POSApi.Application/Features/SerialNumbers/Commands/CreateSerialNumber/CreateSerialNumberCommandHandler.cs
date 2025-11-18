using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.SerialNumbers.Commands.CreateSerialNumber;

public class CreateSerialNumberCommandHandler : ICommandHandler<CreateSerialNumberCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateSerialNumberCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateSerialNumberCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists
        var product = await _unitOfWork.Context.Products
            .FindAsync(new object[] { request.ProductId }, cancellationToken);

        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        // Check if serial number already exists
        var existingSerialNumber = await _unitOfWork.Context.SerialNumbers
            .FirstOrDefaultAsync(sn => sn.Number == request.Number, cancellationToken);

        if (existingSerialNumber != null)
            throw new InvalidOperationException($"Serial number '{request.Number}' already exists");

        // Validate batch if specified
        if (request.BatchId.HasValue)
        {
            var batch = await _unitOfWork.Context.Batches
                .FindAsync(new object[] { request.BatchId.Value }, cancellationToken);

            if (batch == null)
                throw new InvalidOperationException($"Batch with ID {request.BatchId.Value} not found");

            if (batch.ProductId != request.ProductId)
                throw new InvalidOperationException("Batch product does not match serial number product");
        }

        // Validate location if specified
        if (request.LocationId.HasValue)
        {
            var location = await _unitOfWork.Context.Locations
                .FindAsync(new object[] { request.LocationId.Value }, cancellationToken);

            if (location == null)
                throw new InvalidOperationException($"Location with ID {request.LocationId.Value} not found");
        }

        var serialNumber = new SerialNumber(
            request.Number,
            request.ProductId,
            request.BatchId,
            request.LocationId,
            request.WarrantyMonths,
            request.Notes
        );

        _unitOfWork.Context.SerialNumbers.Add(serialNumber);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return serialNumber.Id;
    }
}
