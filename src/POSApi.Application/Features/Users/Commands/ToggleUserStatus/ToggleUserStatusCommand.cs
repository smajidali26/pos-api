using FluentValidation;
using MediatR;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Users.Commands.ToggleUserStatus;

public class ToggleUserStatusCommand : ICommand<Unit>
{
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
}

public class ToggleUserStatusCommandValidator : AbstractValidator<ToggleUserStatusCommand>
{
    public ToggleUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
