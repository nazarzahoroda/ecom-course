namespace EcomCourse.Application
{
    public sealed class CategoryTreeItemDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public List<CategoryTreeItemDto> Children { get; init; } = new();
    }
}
