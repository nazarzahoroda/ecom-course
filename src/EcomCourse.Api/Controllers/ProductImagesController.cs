using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Commands.ConfirmProductImageUpload;
using EcomCourse.Application.Products.Commands.DeleteProductImage;
using EcomCourse.Application.Products.Commands.InitiateProductImageUpload;
using EcomCourse.Application.Products.Queries.GetProductImages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductImagesController : ControllerBase
    {
        private readonly ISender _sender;

        public ProductImagesController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetImages(
            Guid productId,
            CancellationToken cancellationToken
        )
        {
            var request = new GetProductImagesQuery(productId);
            var images = await _sender.Send(request, cancellationToken);
            return Ok(images.Value);
        }

        [HttpPost("{productId}/initiate-upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitiateUpload(
            [FromRoute] Guid productId,
            [FromBody] InitiateUploadRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new InitiateProductImageUploadCommand(
                productId,
                request.FileName,
                request.ContentType
            );
            var result = await _sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(
                    new ProblemDetails
                    {
                        Title = result.Error.Code,
                        Detail = result.Error.Description,
                    }
                );
        }

        [HttpPost("{productId}/confirm-upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmUpload(
            [FromRoute] Guid productId,
            [FromBody] ConfirmUploadRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new ConfirmProductImageUploadCommand(
                productId,
                request.BlobName,
                request.ContentType,
                request.IsMain
            );
            var result = await _sender.Send(command, cancellationToken);

            return result.IsSuccess
                ? Ok(new { ImageId = result.Value })
                : BadRequest(
                    new ProblemDetails
                    {
                        Title = result.Error.Code,
                        Detail = result.Error.Description,
                    }
                );
        }

        [HttpDelete("{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(
            Guid productId,
            Guid imageId,
            CancellationToken cancellationToken
        )
        {
            var result = await _sender.Send(
                new DeleteProductImageCommand(productId, imageId),
                cancellationToken
            );

            if (result.IsFailure)
                return NotFound(
                    new ProblemDetails
                    {
                        Title = result.Error.Code,
                        Detail = result.Error.Description,
                    }
                );

            return NoContent();
        }
    }
}
