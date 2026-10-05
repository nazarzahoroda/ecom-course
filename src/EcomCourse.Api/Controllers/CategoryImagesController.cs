using EcomCourse.Api.Common;
using EcomCourse.Application.Categories.Commands.ConfirmCategoryImageUpload;
using EcomCourse.Application.Categories.Commands.DeleteCategoryImage;
using EcomCourse.Application.Categories.Commands.InitiateCategoryImageUpload;
using EcomCourse.Application.Products;
using EcomCourse.Application.Products.Queries.GetProductImages;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryImagesController : ControllerBase
    {
        public sealed record ConfirmUploadRequest(string BlobName);

        private readonly ISender _sender;

        public CategoryImagesController(ISender sender)
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

        [HttpPost("{categoryId}/initiate-upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> InitiateUpload(
            [FromRoute] Guid categoryId,
            [FromBody] InitiateUploadRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new InitiateCategoryImageUploadCommand(
                categoryId,
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

        [HttpPost("{categoryId}/confirm-upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ConfirmUpload(
            [FromRoute] Guid categoryId,
            [FromBody] ConfirmUploadRequest request,
            CancellationToken cancellationToken
        )
        {
            var command = new ConfirmCategoryImageUploadCommand(categoryId, request.BlobName);
            var result = await _sender.Send(command, cancellationToken);
            if (result.IsFailure)
            {
                return result.ToProblemDetails();
            }
            return Ok();
        }

        [HttpDelete("{categoryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteImage(
            [FromRoute] Guid categoryId,
            CancellationToken cancellationToken
        )
        {
            var result = await _sender.Send(
                new DeleteCategoryImageCommand(categoryId),
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
