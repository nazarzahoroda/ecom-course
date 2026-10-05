using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Categories
{
    public sealed class Category : Entity<Guid>
    {
        private Category()
            : base(Guid.Empty) { }

        private Category(Guid id, string name, string? blobName = null)
            : base(id)
        {
            Name = name;
            BlobName = blobName;
        }

        public string Name { get; private set; } = null!;

        public string? BlobName { get; private set; }

        public static Result<Category> Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<Category>(CategoryErrors.NameEmpty);
            }

            if (name.Length > 100)
            {
                return Result.Failure<Category>(CategoryErrors.NameTooLong);
            }

            var category = new Category(Guid.NewGuid(), name.Trim());

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

        public Result UpdateImage(string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName))
            {
                return Result.Failure(CategoryErrors.BlobNameEmpty);
            }

            BlobName = blobName.Trim();

            return Result.Success();
        }

        public void RemoveImage()
        {
            BlobName = null;
        }
    }
}
