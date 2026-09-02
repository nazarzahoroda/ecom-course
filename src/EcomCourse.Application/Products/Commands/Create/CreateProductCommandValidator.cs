using FluentValidation;

namespace EcomCourse.Application.Products.Commands.Create;

public sealed class CreateProductCommandValidator
    : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Amount)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Currency)
            .IsInEnum();

        RuleFor(x => x.SKU)
            .NotEmpty()
            .Matches(@"^[A-Z]{3}-\d{4}$");

        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}
