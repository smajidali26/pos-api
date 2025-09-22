using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Returns.Commands.CreateReturn;

public class CreateReturnCommand : ICommand<Guid>
{
    public Guid OriginalOrderId { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public ReturnReason Reason { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<ReturnItemRequest> Items { get; set; } = new();
}

public class ReturnItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CreateReturnCommandValidator : AbstractValidator<CreateReturnCommand>
{
    public CreateReturnCommandValidator()
    {
        RuleFor(x => x.OriginalOrderId)
            .NotEmpty().WithMessage("Original Order ID is required");

        RuleFor(x => x.ProcessedByUserId)
            .NotEmpty().WithMessage("Processed By User ID is required");

        RuleFor(x => x.Reason)
            .IsInEnum().WithMessage("Valid return reason is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one return item is required");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("Product ID is required");

            item.RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            item.RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price must be greater than or equal to 0");

            item.RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Return item reason cannot exceed 500 characters");
        });
    }
}