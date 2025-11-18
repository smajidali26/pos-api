using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Payments.Commands.ProcessCashPayment;

public class ProcessCashPaymentCommandHandler : ICommandHandler<ProcessCashPaymentCommand, Payment>
{
    private readonly IUnitOfWork _unitOfWork;

    public ProcessCashPaymentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Payment> Handle(ProcessCashPaymentCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {request.OrderId} not found");
        }

        var paymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";
        var payment = new Payment(request.OrderId, paymentNumber, request.Amount, PaymentMethod.Cash, Guid.NewGuid());

        payment.Authorize("CASH-AUTH", $"CASH-{Guid.NewGuid()}");
        payment.Capture();

        // Update order
        order.Complete(PaymentMethod.Cash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return payment;
    }
}
