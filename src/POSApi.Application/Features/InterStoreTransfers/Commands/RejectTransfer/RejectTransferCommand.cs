using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.RejectTransfer;

public class RejectTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public Guid RejectedByUserId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class RejectTransferCommandValidator : AbstractValidator<RejectTransferCommand>
{
    public RejectTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.RejectedByUserId)
            .NotEmpty().WithMessage("Rejected By User ID is required");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MaximumLength(1000).WithMessage("Reason must not exceed 1000 characters");
    }
}

public class RejectTransferCommandHandler : ICommandHandler<RejectTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public RejectTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(RejectTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            throw new InvalidOperationException($"Transfer with ID {request.TransferId} not found");
        }

        transfer.Reject(request.RejectedByUserId, request.Reason);
        _unitOfWork.InterStoreTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
