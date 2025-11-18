using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Commands.RecallBatch;

public class RecallBatchCommand : ICommand
{
    public Guid BatchId { get; set; }
    public string RecallReason { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class RecallBatchCommandValidator : AbstractValidator<RecallBatchCommand>
{
    public RecallBatchCommandValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required");

        RuleFor(x => x.RecallReason)
            .NotEmpty().WithMessage("Recall reason is required")
            .MaximumLength(500).WithMessage("Recall reason must not exceed 500 characters");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
