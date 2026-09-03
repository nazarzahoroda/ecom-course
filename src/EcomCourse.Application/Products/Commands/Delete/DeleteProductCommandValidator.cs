using FluentValidation;

namespace EcomCourse.Application.Products.Commands.Delete;

public sealed class DeleteProductCommandValidator
    : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(command => command.Id)
            .NotEmpty();
    }
}
