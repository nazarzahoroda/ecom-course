using EcomCourse.Api.Common;
using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Commands.ConfirmProductImageUpload;
using EcomCourse.Application.Products.Commands.DeleteProductImage;
using EcomCourse.Application.Products.Commands.InitiateProductImageUpload;
using EcomCourse.Application.Products.Queries.GetProductImages;
using EcomCourse.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

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
            if (images.IsFailure)
            {
                return images.ToProblemDetails();
            }
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

            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            return Ok(result.Value);
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
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            return Ok(new { ImageId = result.Value });
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
            {
                return result.ToProblemDetails();
            }

            return NoContent();
        }
    }
}
