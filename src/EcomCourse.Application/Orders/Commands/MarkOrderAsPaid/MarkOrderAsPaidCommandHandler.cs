using EcomCourse.Application.Abstractions.Messaging;
using EcomCourse.Domain.Common;
using EcomCourse.Domain.Orders;

namespace EcomCourse.Application.Orders.Commands.MarkOrderAsPaid;

public sealed class MarkOrderAsPaidCommandHandler : ICommandHandler<MarkOrderAsPaidCommand>
{
    private readonly IOrderRepository _orderRepository;

    public MarkOrderAsPaidCommandHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result> Handle(
        MarkOrderAsPaidCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken);

        if (order is null)
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var result = order.MarkAsPaid();

        if (result.IsFailure)
        {
            return result;
        }

        await _orderRepository.UpdateAsync(order, cancellationToken);

        return Result.Success();
    }
}
