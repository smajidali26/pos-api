using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Commands.UpdateVendorStatus;

public class UpdateVendorStatusCommandHandler : ICommandHandler<UpdateVendorStatusCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorStatusCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateVendorStatusCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.VendorId, cancellationToken);
        if (vendor == null)
        {
            throw new InvalidOperationException($"Vendor with ID {request.VendorId} not found");
        }

        switch (request.Status)
        {
            case VendorStatus.Active:
                vendor.Activate();
                break;
            case VendorStatus.Inactive:
                vendor.Deactivate();
                break;
            case VendorStatus.Blocked:
                if (string.IsNullOrEmpty(request.Reason))
                {
                    throw new InvalidOperationException("Reason is required when blocking a vendor");
                }
                vendor.Block(request.Reason);
                break;
            default:
                vendor.UpdateStatus(request.Status);
                break;
        }

        await _unitOfWork.Vendors.UpdateAsync(vendor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}