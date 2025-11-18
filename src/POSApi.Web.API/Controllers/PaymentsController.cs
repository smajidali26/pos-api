using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Features.Payments.Commands.CreatePaymentIntent;
using POSApi.Application.Features.Payments.Commands.ProcessCashPayment;
using POSApi.Application.Features.Payments.Commands.ProcessRefund;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Services;

namespace POSApi.Web.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(
        IMediator mediator,
        ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
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
        var command = new CreatePaymentIntentCommand
        {
            Amount = request.Amount,
            Currency = request.Currency,
            OrderId = request.OrderId
        };

        var result = await _mediator.Send(command, cancellationToken);

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
        try
        {
            var command = new ProcessCashPaymentCommand
            {
                OrderId = request.OrderId,
                Amount = request.Amount
            };

            var payment = await _mediator.Send(command, cancellationToken);

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
        var command = new ProcessRefundCommand
        {
            PaymentId = paymentId,
            TransactionId = request.TransactionId,
            Amount = request.Amount,
            Reason = request.Reason
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result);
    }
}
