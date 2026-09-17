using FluentValidation;

namespace EcomCourse.Application.Orders.Commands.CreateOrder
{
    public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderCommandValidator()
        {
            RuleFor(x => x.customerId)
                .NotEmpty().WithMessage("Customer ID is required.");

            RuleFor(x => x.items)
                .NotEmpty().WithMessage("Order must contain at least one item.");

            RuleForEach(x => x.items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .NotEmpty().WithMessage("ProductId is required.");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0.")
                    .LessThanOrEqualTo(1000).WithMessage("Quantity cannot exceed 1000 units.");

                item.RuleFor(i => i.UnitPrice)
                    .GreaterThan(0).WithMessage("Unit price must be greater than 0.");
            });
        }
    }
}