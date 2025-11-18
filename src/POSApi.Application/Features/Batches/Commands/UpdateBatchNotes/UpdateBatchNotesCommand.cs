using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Batches.Commands.UpdateBatchNotes;

public class UpdateBatchNotesCommand : ICommand
{
    public Guid BatchId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateBatchNotesCommandValidator : AbstractValidator<UpdateBatchNotesCommand>
{
    public UpdateBatchNotesCommandValidator()
    {
        RuleFor(x => x.BatchId)
            .NotEmpty().WithMessage("Batch ID is required");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notes must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
