using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Features.Promotions.Commands.CalculateDiscount;
using POSApi.Application.Features.Promotions.Commands.CreatePromotion;
using POSApi.Application.Features.Promotions.Commands.ValidateCoupon;
using POSApi.Application.Features.Promotions.Queries.GetActivePromotions;
using POSApi.Application.Features.Promotions.Queries.GetPromotionAnalytics;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PromotionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PromotionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all active promotions
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<PromotionDto>>> GetActivePromotions(CancellationToken cancellationToken)
    {
        var query = new GetActivePromotionsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new promotion
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> CreatePromotion([FromBody] CreatePromotionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetActivePromotions), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Validate coupon code
    /// </summary>
    [HttpPost("validate-coupon")]
    public async Task<ActionResult<CouponValidationResult>> ValidateCoupon([FromBody] ValidateCouponCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new CouponValidationResult
            {
                IsValid = false,
                Message = ex.Message
            });
        }
    }

    /// <summary>
    /// Calculate discount for order
    /// </summary>
    [HttpPost("calculate-discount")]
    public async Task<ActionResult<DiscountCalculationResult>> CalculateDiscount([FromBody] CalculateDiscountCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Ok(new DiscountCalculationResult
            {
                HasErrors = true,
                ErrorMessages = new List<string> { ex.Message },
                OriginalAmount = command.OrderAmount,
                FinalAmount = command.OrderAmount
            });
        }
    }

    /// <summary>
    /// Get promotion analytics
    /// </summary>
    [HttpGet("analytics")]
    public async Task<ActionResult<PromotionAnalyticsDto>> GetPromotionAnalytics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, CancellationToken cancellationToken)
    {
        var query = new GetPromotionAnalyticsQuery
        {
            StartDate = startDate,
            EndDate = endDate
        };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}