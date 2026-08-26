using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Commands.CancelOrder;

public sealed class CancelOrderCommandHandler : ICommandHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository;

    public CancelOrderCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(
        CancelOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var result = order.Cancel();

        if (result.IsFailure)
        {
            return result;
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}
