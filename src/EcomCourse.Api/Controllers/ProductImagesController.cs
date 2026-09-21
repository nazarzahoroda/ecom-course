using EcomCourse.Application.Products.Commands.DeleteProductImage;
using EcomCourse.Application.Products.Commands.UploadProductImage;
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

        [HttpGet]
        public async Task<IActionResult> GetImages(
            Guid productId,
            CancellationToken cancellationToken
        )
        {
            var request = new GetProductImagesQuery(productId);
            var images = await _sender.Send(request, cancellationToken);
            return Ok(images);
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(
            Guid productId,
            IFormFile file,
            [FromQuery] bool isMain,
            CancellationToken cancellationToken
        )
        {
            if (file is null || file.Length == 0)
                return BadRequest(new ProblemDetails { Detail = "Файл відсутній або порожній." });

            await using var stream = file.OpenReadStream();
            var command = new UploadProductImageCommand(
                productId,
                stream,
                file.FileName,
                file.ContentType,
                isMain
            );
            var result = await _sender.Send(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = result.Error.Code,
                        Detail = result.Error.Description,
                    }
                );

            return Ok(new { ImageId = result.Value });
        }

        [HttpDelete("{imageId:guid}")]
        //[Authorize(Roles = "Admin")]
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
