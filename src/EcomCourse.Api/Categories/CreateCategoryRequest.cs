namespace EcomCourse.Api.Categories;

public sealed record CreateCategoryRequest(
    string Name,
    Guid? ParentId = null);
