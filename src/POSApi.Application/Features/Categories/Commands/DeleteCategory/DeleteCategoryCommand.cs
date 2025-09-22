using FluentValidation;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommand : ICommand
{
    public Guid Id { get; set; }
}

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Category ID is required");
    }
}