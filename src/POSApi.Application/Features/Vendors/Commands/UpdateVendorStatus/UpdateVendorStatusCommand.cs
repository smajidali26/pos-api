using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Vendors.Commands.UpdateVendorStatus;

public class UpdateVendorStatusCommand : ICommand
{
    public Guid VendorId { get; set; }
    public VendorStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class UpdateVendorStatusCommandValidator : AbstractValidator<UpdateVendorStatusCommand>
{
    public UpdateVendorStatusCommandValidator()
    {
        RuleFor(x => x.VendorId)
            .NotEmpty().WithMessage("Vendor ID is required");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid vendor status");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required when blocking a vendor")
            .When(x => x.Status == VendorStatus.Blocked);
    }
}