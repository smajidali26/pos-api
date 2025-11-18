using FluentValidation;
using MediatR;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Returns.Commands.ProcessReturn;

public class ProcessReturnCommand : ICommand<Unit>
{
    public Guid ReturnId { get; set; }
    public RefundMethod RefundMethod { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ProcessReturnCommandValidator : AbstractValidator<ProcessReturnCommand>
{
    public ProcessReturnCommandValidator()
    {
        RuleFor(x => x.ReturnId)
            .NotEmpty().WithMessage("Return ID is required");

        RuleFor(x => x.RefundMethod)
            .IsInEnum().WithMessage("Valid refund method is required");

        RuleFor(x => x.Notes)
            .MaximumLength(2000).WithMessage("Notes cannot exceed 2000 characters");
    }
}
