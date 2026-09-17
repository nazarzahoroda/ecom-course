using FluentValidation;

namespace EcomCourse.Application.Carts.Commands.UpdateCartItemQuantityCommand
{
    public class UpdateCartItemQuantityCommandValidator : AbstractValidator<UpdateCartItemQuantityCommand>
    {
        public UpdateCartItemQuantityCommandValidator()
        {
            RuleFor(x => x.dto.ProductId)
                .NotEmpty().WithMessage("ProductId is required.");

            RuleFor(x => x.dto.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 units per item.");
        }
    }
}