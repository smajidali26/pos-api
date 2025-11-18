using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Batches.Commands.CreateBatch;

public class CreateBatchCommandHandler : ICommandHandler<CreateBatchCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateBatchCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateBatchCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists
        var product = await _unitOfWork.Context.Products
            .FindAsync(new object[] { request.ProductId }, cancellationToken);

        if (product == null)
            throw new InvalidOperationException($"Product with ID {request.ProductId} not found");

        // Check if batch number already exists
        var existingBatch = await _unitOfWork.Context.Batches
            .FirstOrDefaultAsync(b => b.BatchNumber == request.BatchNumber, cancellationToken);

        if (existingBatch != null)
            throw new InvalidOperationException($"Batch with number '{request.BatchNumber}' already exists");

        // Validate vendor if specified
        if (request.VendorId.HasValue)
        {
            var vendor = await _unitOfWork.Context.Vendors
                .FindAsync(new object[] { request.VendorId.Value }, cancellationToken);

            if (vendor == null)
                throw new InvalidOperationException($"Vendor with ID {request.VendorId.Value} not found");
        }

        // Validate location if specified
        if (request.LocationId.HasValue)
        {
            var location = await _unitOfWork.Context.Locations
                .FindAsync(new object[] { request.LocationId.Value }, cancellationToken);

            if (location == null)
                throw new InvalidOperationException($"Location with ID {request.LocationId.Value} not found");
        }

        var batch = new Batch(
            request.BatchNumber,
            request.ProductId,
            request.VendorId,
            request.PurchaseOrderId,
            request.LocationId,
            request.InitialQuantity,
            request.UnitCost,
            request.ManufactureDate,
            request.ExpiryDate,
            request.Notes
        );

        _unitOfWork.Context.Batches.Add(batch);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return batch.Id;
    }
}
