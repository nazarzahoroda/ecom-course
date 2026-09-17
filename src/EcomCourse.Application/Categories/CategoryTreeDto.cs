namespace EcomCourse.Application.Categories;

public sealed record CategoryTreeDto(
    Guid Id,
    string Name,
    List<CategoryTreeDto> Children);
