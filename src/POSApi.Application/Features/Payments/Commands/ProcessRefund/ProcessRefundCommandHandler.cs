using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Services;

namespace POSApi.Application.Features.Payments.Commands.ProcessRefund;

public class ProcessRefundCommandHandler : ICommandHandler<ProcessRefundCommand, PaymentGatewayResult>
{
    private readonly IPaymentGatewayService _paymentGatewayService;

    public ProcessRefundCommandHandler(IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<PaymentGatewayResult> Handle(ProcessRefundCommand request, CancellationToken cancellationToken)
    {
        var result = await _paymentGatewayService.ProcessRefundAsync(
            request.TransactionId,
            request.Amount,
            request.Reason,
            cancellationToken);

        return result;
    }
}
