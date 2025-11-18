using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Commands.MarkBatchExpired;

public class MarkBatchExpiredCommand : ICommand
{
    public Guid BatchId { get; set; }
}

public class MarkBatchExpiredCommandValidator : AbstractValidator<MarkBatchExpiredCommand>
{
    public MarkBatchExpiredCommandValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required");
    }
}
