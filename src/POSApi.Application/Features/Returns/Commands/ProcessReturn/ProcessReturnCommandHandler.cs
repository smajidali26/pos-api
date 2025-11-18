using MediatR;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Returns.Commands.ProcessReturn;

public class ProcessReturnCommandHandler : ICommandHandler<ProcessReturnCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProcessReturnCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ProcessReturnCommand request, CancellationToken cancellationToken)
    {
        // Get the return from the database
        var return_ = await _unitOfWork.Returns.GetByIdAsync(request.ReturnId, cancellationToken);
        if (return_ == null)
        {
            throw new InvalidOperationException($"Return with ID {request.ReturnId} not found");
        }

        // Process the return based on refund method
        return_.Process(request.RefundMethod);

        // Update notes if provided
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            // Since the Return entity doesn't have a public setter for Notes,
            // we'll rely on the Process method to handle the state change
            // If notes need to be updated separately, the entity would need a method for that
        }

        // Save changes
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
