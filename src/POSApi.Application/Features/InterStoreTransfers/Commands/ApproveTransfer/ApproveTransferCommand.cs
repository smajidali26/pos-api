using FluentValidation;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.InterStoreTransfers.Commands.ApproveTransfer;

public class ApproveTransferCommand : ICommand
{
    public Guid TransferId { get; set; }
    public Guid ApprovedByUserId { get; set; }
}

public class ApproveTransferCommandValidator : AbstractValidator<ApproveTransferCommand>
{
    public ApproveTransferCommandValidator()
    {
        RuleFor(x => x.TransferId)
            .NotEmpty().WithMessage("Transfer ID is required");

        RuleFor(x => x.ApprovedByUserId)
            .NotEmpty().WithMessage("Approved By User ID is required");
    }
}

public class ApproveTransferCommandHandler : ICommandHandler<ApproveTransferCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveTransferCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ApproveTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _unitOfWork.InterStoreTransfers.GetByIdAsync(request.TransferId, cancellationToken);
        if (transfer == null)
        {
            throw new InvalidOperationException($"Transfer with ID {request.TransferId} not found");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(request.ApprovedByUserId, cancellationToken);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {request.ApprovedByUserId} not found");
        }

        transfer.Approve(request.ApprovedByUserId);
        _unitOfWork.InterStoreTransfers.Update(transfer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
