using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Abstractions;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.DeleteProductImage
{
    public class DeleteProductImageCommandHandler : ICommandHandler<DeleteProductImageCommand>
    {
        private readonly IProductManager _productService;

        public DeleteProductImageCommandHandler(IProductManager productService)
        {
            _productService = productService;
        }

        public async Task<Result> Handle(
            DeleteProductImageCommand request,
            CancellationToken cancellationToken
        )
        {
            var deleteResult = await _productService.DeleteImageAsync(
                request.productId,
                request.imageId,
                cancellationToken
            );
            return deleteResult;
        }
    }
}
