using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Interfaces;
using EcomCourse.Application.Products.Services;
using EcomCourse.Domain.Common;

namespace EcomCourse.Application.Products.Commands.DeleteProductImage
{
    public class DeleteProductImageCommandHandler : ICommandHandler<DeleteProductImageCommand>
    {
        private readonly IProductService _productService;

        public DeleteProductImageCommandHandler(IProductService productService)
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
