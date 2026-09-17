using FluentValidation;

namespace EcomCourse.Application.Carts.Commands.AddItemToCartCommand
{
    public class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
    {
        public AddItemToCartCommandValidator()
        {
            RuleFor(x => x.dto.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.dto.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 units per item.");
        }
    }
}
