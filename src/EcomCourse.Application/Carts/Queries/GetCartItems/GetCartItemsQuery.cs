using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Application.Carts.DTOs;

namespace EcomCourse.Application.Carts.Queries.GetCartItems
{
    public record GetCartItemsQuery : IQuery<CartDetailsDto>;
}
