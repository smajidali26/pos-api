using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Services;

namespace POSApi.Application.Features.Payments.Commands.CreatePaymentIntent;

public class CreatePaymentIntentCommandHandler : ICommandHandler<CreatePaymentIntentCommand, PaymentIntentResult>
{
    private readonly IPaymentGatewayService _paymentGatewayService;

    public CreatePaymentIntentCommandHandler(IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService;
    }

    public async Task<PaymentIntentResult> Handle(CreatePaymentIntentCommand request, CancellationToken cancellationToken)
    {
        var result = await _paymentGatewayService.CreatePaymentIntentAsync(
            request.Amount,
            request.Currency ?? "USD",
            request.OrderId.ToString(),
            cancellationToken);

        return result;
    }
}
