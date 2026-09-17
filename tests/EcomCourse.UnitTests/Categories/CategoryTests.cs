using EcomCourse.Domain;
using EcomCourse.Domain.Categories;

namespace EcomCourse.UnitTests.Categories
{
    public class CategoryTests
    {
        [Fact]
        public void Create_WithValidName_ShouldReturnSuccess()
        {
            var name = "Electronics";

            var result = Category.Create(name);

            Assert.True(result.IsSuccess);
            Assert.Equal(name, result.Value!.Name);
        }

        [Fact]
        public void Create_WithParentId_ShouldSetParentId()
        {
            var name = "Smartphones";
            var parentId = Guid.NewGuid();

            var result = Category.Create(name, parentId);

            Assert.True(result.IsSuccess);
            Assert.Equal(parentId, result.Value!.ParentId);
        }

        [Fact]
        public void Create_WithEmptyName_ShouldReturnFailure()
        {
            var name = "";

            var result = Category.Create(name);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryErrors.NameEmpty, result.Error);
        }

        [Fact]
        public void Create_WithNameTooLong_ShouldReturnFailure()
        {
            var name = new string('A', 101);

            var result = Category.Create(name);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryErrors.NameTooLong, result.Error);
        }

        [Fact]
        public void UpdateName_WithValidName_ShouldReturnSuccess()
        {
            var category = Category.Create("Electronics").Value!;
            var newName = "Smartphones";

            var result = category.UpdateName(newName);

            Assert.True(result.IsSuccess);
            Assert.Equal(newName, category.Name);
        }

        [Fact]
        public void UpdateName_WithEmptyName_ShouldReturnFailure()
        {
            var originalName = "Electronics";
            var category = Category.Create(originalName).Value!;

            var result = category.UpdateName("");

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryErrors.NameEmpty, result.Error);
            Assert.Equal(originalName, category.Name);
        }

        [Fact]
        public void UpdateName_WithNameTooLong_ShouldReturnFailure()
        {
            var originalName = "Electronics";
            var category = Category.Create(originalName).Value!;
            var newName = new string('A', 101);

            var result = category.UpdateName(newName);

            Assert.True(result.IsFailure);
            Assert.Equal(CategoryErrors.NameTooLong, result.Error);
            Assert.Equal(originalName, category.Name);
        }

        [Fact]
        public void UpdateParent_WithOwnId_ShouldReturnFailure()
        {
            var category = Category.Create("Electronics").Value!;

            var result = category.UpdateParent(category.Id);

            Assert.True(result.IsFailure);
            Assert.Null(category.ParentId);
        }

        [Fact]
        public void UpdateParent_WithDifferentParentId_ShouldReturnSuccess()
        {
            var category = Category.Create("Smartphones").Value!;
            var parentId = Guid.NewGuid();

            var result = category.UpdateParent(parentId);

            Assert.True(result.IsSuccess);
            Assert.Equal(parentId, category.ParentId);
        }

        [Fact]
        public void UpdateParent_WithNull_ShouldRemoveParent()
        {
            var parentId = Guid.NewGuid();
            var category = Category.Create(
                "Smartphones",
                parentId).Value!;

            var result = category.UpdateParent(null);

            Assert.True(result.IsSuccess);
            Assert.Null(category.ParentId);
        }
    }
}
