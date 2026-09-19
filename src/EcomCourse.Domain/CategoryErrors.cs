using EcomCourse.Domain.Common;

namespace EcomCourse.Domain
{
    public static class CategoryErrors
    {
        public static readonly DomainError NameEmpty = new(
            "Category.NameEmpty",
            "Category name cannot be empty.",
            ErrorType.Validation);

        public static readonly DomainError NameTooLong = new(
            "Category.NameTooLong",
            "Category name cannot exceed 100 characters.",
            ErrorType.Validation);

        public static DomainError NotFound(Guid id) => new(
            "Category.NotFound",
            $"Category '{id}' was not found.",
            ErrorType.NotFound);
    }
}
