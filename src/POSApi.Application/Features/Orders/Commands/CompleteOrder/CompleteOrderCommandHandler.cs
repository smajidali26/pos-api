using POSApi.Infrastructure.Persistence;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Orders.Commands.CompleteOrder;

public class CompleteOrderCommandHandler : ICommandHandler<CompleteOrderCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public CompleteOrderCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(CompleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");
        }

        if (order.Status != Domain.Entities.OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Order {order.OrderNumber} is not in pending status");
        }

        order.Complete(request.PaymentMethod, request.Notes, request.CashAmount, request.CardAmount, request.ChangeAmount);

        // Add loyalty points if customer exists (1 point per dollar spent)
        if (order.Customer != null)
        {
            var loyaltyPoints = Math.Floor(order.TotalAmount);
            order.Customer.AddLoyaltyPoints(loyaltyPoints);
            await _unitOfWork.Customers.UpdateAsync(order.Customer, cancellationToken);
        }

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}