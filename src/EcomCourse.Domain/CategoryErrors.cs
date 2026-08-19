using EcomCourse.Domain.Common;

namespace EcomCourse.Domain
{
    public static class CategoryErrors
    {
        public static readonly DomainError NameEmpty = new(
            "Category.NameEmpty",
            "Category name cannot be empty.");

        public static readonly DomainError NameTooLong = new(
            "Category.NameTooLong",
            "Category name cannot exceed 101 characters.");

        public static DomainError NotFound(Guid id) => new(
            "Category.NotFound",
            $"Category '{id}' was not found.");
    }
}
