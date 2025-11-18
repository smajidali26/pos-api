using FluentValidation;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.CreateTransfer;

public class CreateTransferCommand : ICommand<Guid>
{
    public Guid FromStoreId { get; set; }
    public Guid ToStoreId { get; set; }
    public Guid RequestedByUserId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<TransferItemRequest> Items { get; set; } = new();
}

public class CreateTransferCommandValidator : AbstractValidator<CreateTransferCommand>
{
    public CreateTransferCommandValidator()
    {
        RuleFor(x => x.FromStoreId)
            .NotEmpty().WithMessage("From Store ID is required");

        RuleFor(x => x.ToStoreId)
            .NotEmpty().WithMessage("To Store ID is required")
            .NotEqual(x => x.FromStoreId).WithMessage("From Store and To Store cannot be the same");

        RuleFor(x => x.RequestedByUserId)
            .NotEmpty().WithMessage("Requested By User ID is required");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("At least one item is required")
            .Must(items => items.All(i => i.RequestedQuantity > 0))
            .WithMessage("All items must have a positive quantity");
    }
}

public class CreateTransferCommandHandler : ICommandHandler<CreateTransferCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        // Validate stores exist
        var fromStore = await _unitOfWork.Stores.GetByIdAsync(request.FromStoreId, cancellationToken);
        if (fromStore == null)
        {
            throw new InvalidOperationException($"From Store with ID {request.FromStoreId} not found");
        }

        var toStore = await _unitOfWork.Stores.GetByIdAsync(request.ToStoreId, cancellationToken);
        if (toStore == null)
        {
            throw new InvalidOperationException($"To Store with ID {request.ToStoreId} not found");
        }

        // Validate user exists
        var user = await _unitOfWork.Users.GetByIdAsync(request.RequestedByUserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.RequestedByUserId} not found");
        }

        // Generate transfer number
        var transferNumber = $"TR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        // Create transfer
        var transfer = new InterStoreTransfer(
            transferNumber,
            request.FromStoreId,
            request.ToStoreId,
            request.RequestedByUserId,
            request.Notes);

        // Add items
        foreach (var item in request.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                throw new InvalidOperationException($"Product with ID {item.ProductId} not found");
            }

            transfer.AddItem(item.ProductId, item.RequestedQuantity, item.UnitCost);
        }

        await _unitOfWork.InterStoreTransfers.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return transfer.Id;
    }
}
