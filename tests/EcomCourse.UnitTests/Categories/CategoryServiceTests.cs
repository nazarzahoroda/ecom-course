using EcomCourse.Domain;
using EcomCourse.Domain.Categories;
using EcomCourse.Infrastructure.Persistence;
using EcomCourse.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace EcomCourse.UnitTests.Categories;

public class CategoryManagerTests
{
    [Fact]
    public async Task CreateAsync_WhenNameAlreadyExists_ReturnsNameAlreadyExistsError()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var existingCategory = Category.Create("Electronics").Value!;

        dbContext.Categories.Add(existingCategory);
        await dbContext.SaveChangesAsync();

        var service = new CategoryManager(dbContext);

        // Act
        var result = await service.CreateAsync("Electronics");

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.NameAlreadyExists, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenAnotherCategoryHasSameName_ReturnsNameAlreadyExistsError()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var electronics = Category.Create("Electronics").Value!;
        var books = Category.Create("Books").Value!;

        dbContext.Categories.AddRange(electronics, books);
        await dbContext.SaveChangesAsync();

        var service = new CategoryManager(dbContext);

        // Act
        var result = await service.UpdateAsync(
            books.Id,
            electronics.Name);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.NameAlreadyExists, result.Error);
    }

    [Fact]
    public async Task UpdateAsync_WhenCategoryKeepsItsOwnName_Succeeds()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var category = Category.Create("Electronics").Value!;

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync();

        var service = new CategoryManager(dbContext);

        // Act
        var result = await service.UpdateAsync(
            category.Id,
            category.Name);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateAsync_WhenNameAlreadyExists_DoesNotChangeCategoryName()
    {
        // Arrange
        await using var dbContext = CreateDbContext();

        var electronics = Category.Create("Electronics").Value!;
        var books = Category.Create("Books").Value!;

        dbContext.Categories.AddRange(electronics, books);
        await dbContext.SaveChangesAsync();

        var service = new CategoryManager(dbContext);

        // Act
        var result = await service.UpdateAsync(
            books.Id,
            electronics.Name);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(CategoryErrors.NameAlreadyExists, result.Error);
        Assert.Equal("Books", books.Name);
    }

    private static EcomCourseDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EcomCourseDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new EcomCourseDbContext(options);
    }
}
