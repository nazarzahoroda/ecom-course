using EcomCourse.Application.Carts.Commands.AddItemToCartCommand;
using EcomCourse.Application.Carts.Commands.CartCheckout;
using EcomCourse.Application.Carts.Commands.RemoveItemFromCartCommand;
using EcomCourse.Application.Carts.Commands.UpdateCartItemQuantityCommand;
using EcomCourse.Application.Carts.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcomCourse.Api.Controllers
{
    [Authorize]
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
            [FromBody] AddItemToCartDto dto,
            CancellationToken cancellationToken
        )
        {
            var request = new AddItemToCartCommand(dto);
            var result = await _sender.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = result.Error.Code,
                        Detail = result.Error.Description,
                        Status = StatusCodes.Status400BadRequest,
                    }
                );
            }
            return Ok();
        }

        [HttpPut("items")]
        public async Task<IActionResult> UpdateItemQuantity(
            [FromBody] UpdateCartItemQuantityDto dto,
            CancellationToken cancellationToken
        )
        {
            var request = new UpdateCartItemQuantityCommand(dto);
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error.Code is "Cart.NotFound" or "CartItem.NotFound"
                    ? NotFound(
                        new ProblemDetails
                        {
                            Title = result.Error.Code,
                            Detail = result.Error.Description,
                            Status = StatusCodes.Status404NotFound,
                        }
                    )
                    : BadRequest(
                        new ProblemDetails
                        {
                            Title = result.Error.Code,
                            Detail = result.Error.Description,
                            Status = StatusCodes.Status400BadRequest,
                        }
                    );
            }
            return Ok();
        }

        [HttpDelete("items/{id}")]
        public async Task<IActionResult> RemoveItem(Guid id, CancellationToken cancellationToken)
        {
            var request = new RemoveItemFromCartCommand(id);
            var result = await _sender.Send(request, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Code == "CartItem.NotFound"
                    ? NotFound(
                        new ProblemDetails
                        {
                            Title = result.Error.Code,
                            Detail = result.Error.Description,
                            Status = StatusCodes.Status404NotFound,
                        }
                    )
                    : BadRequest(
                        new ProblemDetails
                        {
                            Title = result.Error.Code,
                            Detail = result.Error.Description,
                            Status = StatusCodes.Status400BadRequest,
                        }
                    );
            }

            return Ok(result.Value);
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CancellationToken cancellationToken)
        {
            var request = new CartCheckoutCommand();
            var result = await _sender.Send(request, cancellationToken);
            if (result.IsFailure)
            {
                return result.Error.Code == "Cart.NotFound"
                    ? NotFound(
                        new ProblemDetails
                        {
                            Title = result.Error.Code,
                            Detail = result.Error.Description,
                            Status = StatusCodes.Status404NotFound,
                        }
                    )
                    : BadRequest(
                        new ProblemDetails
                        {
                            Title = result.Error.Code,
                            Detail = result.Error.Description,
                            Status = StatusCodes.Status400BadRequest,
                        }
                    );
            }
            return Ok(result.Value);
        }
    }
}
