using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.CancelTransfer;

public class CancelTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class CancelTransferCommandValidator : AbstractValidator<CancelTransferCommand>
{
    public CancelTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters");
    }
}

public class CancelTransferCommandHandler : ICommandHandler<CancelTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CancelTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            throw new InvalidOperationException($"Transfer with ID {request.TransferId} not found");
        }

        transfer.Cancel(request.Reason);
        _unitOfWork.InterStoreTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
