using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Infrastructure.Services;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IPaymentGatewayService paymentGatewayService,
        IUnitOfWork unitOfWork,
        ILogger<PaymentsController> logger)
    {
        _paymentGatewayService = paymentGatewayService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Create a payment intent for client-side processing (requires Stripe configuration)
    /// </summary>
    [HttpPost("create-intent")]
    [ProducesResponseType(typeof(PaymentIntentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<PaymentIntentResult>> CreatePaymentIntent(
        [FromBody] CreatePaymentIntentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _paymentGatewayService.CreatePaymentIntentAsync(
            request.Amount,
            request.Currency ?? "USD",
            request.OrderId.ToString(),
            cancellationToken);

        if (!result.Success)
        {
            // Check if it's a configuration error (Stripe not set up)
            if (result.ErrorMessage?.Contains("not configured") == true)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new {
                        error = result.ErrorMessage,
                        useCashInstead = true
                    });
            }
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }

    /// <summary>
    /// Process a cash payment
    /// </summary>
    [HttpPost("cash")]
    [ProducesResponseType(typeof(Payment), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Payment>> ProcessCashPayment(
        [FromBody] ProcessCashPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken);
        if (order == null)
        {
            return BadRequest(new { error = "Order not found" });
        }

        var paymentNumber = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";
        var payment = new Payment(request.OrderId, paymentNumber, request.Amount, PaymentMethod.Cash, Guid.NewGuid());

        payment.Authorize("CASH-AUTH", $"CASH-{Guid.NewGuid()}");
        payment.Capture();

        // Update order
        order.Complete(PaymentMethod.Cash);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(payment);
    }

    /// <summary>
    /// Process a refund
    /// </summary>
    [HttpPost("{paymentId:guid}/refund")]
    [ProducesResponseType(typeof(PaymentGatewayResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentGatewayResult>> ProcessRefund(
        Guid paymentId,
        [FromBody] ProcessRefundRequest request,
        CancellationToken cancellationToken)
    {
        // Get payment from DB (you'll need to add a method to get payment by ID)
        // For now, assume we have the stripe transaction ID

        var result = await _paymentGatewayService.ProcessRefundAsync(
            request.TransactionId,
            request.Amount,
            request.Reason,
            cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }
}

public record CreatePaymentIntentRequest
{
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public Guid OrderId { get; init; }
}

public record ProcessCashPaymentRequest
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record ProcessRefundRequest
{
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
