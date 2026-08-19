using EcomCourse.Application.Carts.Commands.AddItemToCartCommand;
using EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand;
using EcomCourse.Application.Carts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly ISender _sender;
        public CartController(ISender sender)
        {
            _sender = sender;
        }



       [HttpPost("items")]
       public async Task<IActionResult> AddItem(
           Guid customerId,
       [FromBody] AddItemToCartDto dto,
       CancellationToken cancellationToken)
        {
            var request = new AddItemToCartCommand(customerId, dto);
            var result = await _sender.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return NoContent();
        }
        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveItem(Guid id, CancellationToken cancellationToken)
        {
            var request = new RemoveItemFromCartCommand(id);
            var result = await _sender.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = result.Error.Code,
                    Detail = result.Error.Description,
                    Status = StatusCodes.Status400BadRequest
                });
            }

            return NoContent();
        }
    }
}
