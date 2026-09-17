using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Categories
{
    public sealed class Category
    {
        private Category()
        {

        }

        private Category(Guid id, string name, Guid? parentId)
        {
            Id = id;
            Name = name;
            ParentId = parentId;
        }

        public Guid Id { get; private set; }

        public string Name { get; private set; } = null!;

        public Guid? ParentId { get; private set; }

        public Category? Parent { get; private set; }

        public ICollection<Category> Children { get; private set; }
            = new List<Category>();

        public static Result<Category> Create(
            string name,
            Guid? parentId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<Category>(CategoryErrors.NameEmpty);
            }

            if (name.Length > 100)
            {
                return Result.Failure<Category>(CategoryErrors.NameTooLong);
            }

            var category = new Category(
                Guid.NewGuid(),
                name.Trim(),
                parentId);

            return Result.Success(category);
        }

        public Result UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure(CategoryErrors.NameEmpty);
            }

            if (name.Length > 100)
            {
                return Result.Failure(CategoryErrors.NameTooLong);
            }

            Name = name.Trim();

            return Result.Success();
        }

        public Result UpdateParent(Guid? parentId)
        {
            if (parentId == Id)
            {
                return Result.Failure(
                    CategoryErrors.CyclicReference);
            }

            ParentId = parentId;

            return Result.Success();
        }
    }
}
