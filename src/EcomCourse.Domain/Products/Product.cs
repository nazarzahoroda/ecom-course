using System;
using System.Collections.Generic;
using System.Text;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Primitives;

namespace EcomCourse.Domain.Products
{
    public sealed class Product : Entity<Guid>
    {
        public string Name { get; private set; }
        public Price Price { get; private set; }
        public Sku Sku { get; private set; }
        public Guid CategoryId { get; private set; }

        private Product(Guid id, string name, Price price, Sku sku, Guid categoryId)
            : base(id)
        {
            Name = name;
            Price = price;
            Sku = sku;
            CategoryId = categoryId;
        }

        public static Result<Product> Create(string name, Price price, Sku sku, Guid categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return Result.Failure<Product>(ProductErrors.EmptyName);
            }

            if (categoryId == Guid.Empty)
            {
                return Result.Failure<Product>(new DomainError("Product.EmptyCategory", "CategoryId cannot be empty."));
            }

            return Result.Success(new Product(Guid.NewGuid(), name.Trim(), price, sku, categoryId));
        }
    }
}
