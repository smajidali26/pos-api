using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Stores.Commands.UpdateStoreManager;

public class UpdateStoreManagerCommand : ICommand
{
    public Guid StoreId { get; set; }
    public Guid ManagerUserId { get; set; }
}

public class UpdateStoreManagerCommandValidator : AbstractValidator<UpdateStoreManagerCommand>
{
    public UpdateStoreManagerCommandValidator()
    {
        RuleFor(x => x.StoreId)
            .NotEmpty().WithMessage("Store ID is required");

        RuleFor(x => x.ManagerUserId)
            .NotEmpty().WithMessage("Manager User ID is required");
    }
}
