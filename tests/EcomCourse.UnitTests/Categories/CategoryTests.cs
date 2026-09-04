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
    }
}
