using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Vendors.Commands.CreateVendor;

public class CreateVendorCommandHandler : ICommandHandler<CreateVendorCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateVendorCommand request, CancellationToken cancellationToken)
    {
        // Check if vendor with the same name already exists
        var existingVendor = await _unitOfWork.Vendors.GetByNameAsync(request.Name, cancellationToken);
        if (existingVendor != null)
        {
            throw new InvalidOperationException($"Vendor with name '{request.Name}' already exists");
        }

        // Check if vendor with the same email already exists
        var existingByEmail = await _unitOfWork.Vendors.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail != null)
        {
            throw new InvalidOperationException($"Vendor with email '{request.Email}' already exists");
        }

        // Check if vendor with the same Tax ID already exists (if provided)
        if (!string.IsNullOrEmpty(request.TaxId))
        {
            var existingByTaxId = await _unitOfWork.Vendors.GetByTaxIdAsync(request.TaxId, cancellationToken);
            if (existingByTaxId != null)
            {
                throw new InvalidOperationException($"Vendor with Tax ID '{request.TaxId}' already exists");
            }
        }

        // Create vendor
        var vendor = new Vendor(
            request.Name,
            request.CompanyName,
            request.ContactPerson,
            request.Email,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.State,
            request.ZipCode,
            request.Country,
            request.Type);

        // Update optional properties
        if (!string.IsNullOrEmpty(request.TaxId))
        {
            vendor.UpdateTaxId(request.TaxId);
        }

        if (!string.IsNullOrEmpty(request.Website))
        {
            vendor.UpdateWebsite(request.Website);
        }

        if (!string.IsNullOrEmpty(request.Notes))
        {
            vendor.UpdateNotes(request.Notes);
        }

        vendor.UpdatePaymentTerms(request.PaymentTerms);
        vendor.UpdateCreditLimit(request.CreditLimit);

        await _unitOfWork.Vendors.AddAsync(vendor, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return vendor.Id;
    }
}