using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Commands.UpdateVendor;

public class UpdateVendorCommandHandler : ICommandHandler<UpdateVendorCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Vendors.GetByIdAsync(request.Id, cancellationToken);
        if (vendor == null)
        {
            throw new InvalidOperationException($"Vendor with ID {request.Id} not found");
        }

        // Check if another vendor with the same name already exists (excluding current vendor)
        var existingVendor = await _unitOfWork.Vendors.GetByNameAsync(request.Name, cancellationToken);
        if (existingVendor != null && existingVendor.Id != request.Id)
        {
            throw new InvalidOperationException($"Another vendor with name '{request.Name}' already exists");
        }

        // Check if another vendor with the same email already exists (excluding current vendor)
        var existingByEmail = await _unitOfWork.Vendors.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail != null && existingByEmail.Id != request.Id)
        {
            throw new InvalidOperationException($"Another vendor with email '{request.Email}' already exists");
        }

        // Check if another vendor with the same Tax ID already exists (excluding current vendor)
        if (!string.IsNullOrEmpty(request.TaxId))
        {
            var existingByTaxId = await _unitOfWork.Vendors.GetByTaxIdAsync(request.TaxId, cancellationToken);
            if (existingByTaxId != null && existingByTaxId.Id != request.Id)
            {
                throw new InvalidOperationException($"Another vendor with Tax ID '{request.TaxId}' already exists");
            }
        }

        // Update vendor properties
        vendor.UpdateContactInfo(request.ContactPerson, request.Email, request.PhoneNumber);
        vendor.UpdateAddress(request.Address, request.City, request.State, request.ZipCode, request.Country);
        vendor.UpdatePaymentTerms(request.PaymentTerms);
        vendor.UpdateCreditLimit(request.CreditLimit);
        vendor.UpdateTaxId(request.TaxId);
        vendor.UpdateWebsite(request.Website);
        vendor.UpdateNotes(request.Notes);

        await _unitOfWork.Vendors.UpdateAsync(vendor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}