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
            "Category name cannot exceed 100 characters.");

        public static DomainError NotFound(Guid id) => new(
            "Category.NotFound",
            $"Category '{id}' was not found.");

        public static readonly DomainError CyclicReference = new(
            "Category.CyclicReference",
            "Category cannot reference itself or one of its descendants as parent.");
    }
}
