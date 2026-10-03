using EcomCourse.Application.Abstractions.Messaging;

namespace EcomCourse.Application.Products.Commands.DeleteProductImage
{
    public record DeleteProductImageCommand(Guid productId, Guid imageId) : ICommand;
}
