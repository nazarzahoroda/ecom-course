using FluentValidation;

namespace EcomCourse.Application.Categories.Commands.Update
{
    public sealed class UpdateCategoryCommandValidator
    : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(100);
        }
    }
}
