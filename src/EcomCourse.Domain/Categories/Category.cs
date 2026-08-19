using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Domain.Common;

namespace EcomCourse.Domain.Categories
{
    public sealed class Category
    {
        private Category(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; init; }

        public string Name { get; init; } = null!;

        public static Result<Category> Create(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<Category>(CategoryErrors.NameEmpty);
            }

            if (name.Length > 100)            {
                return Result.Failure<Category>(CategoryErrors.NameTooLong);
            }

            return Result.Success(new Category(Guid.NewGuid(), name.Trim()));
        }

    }
}
