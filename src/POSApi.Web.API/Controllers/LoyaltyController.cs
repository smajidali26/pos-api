using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.DTOs.Loyalty;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Features.Loyalty.Commands.AdjustPoints;
using POSApi.Application.Features.Loyalty.Commands.CreateCustomerTier;
using POSApi.Application.Features.Loyalty.Commands.CreateLoyaltyProgram;
using POSApi.Application.Features.Loyalty.Commands.CreateReward;
using POSApi.Application.Features.Loyalty.Commands.EarnPoints;
using POSApi.Application.Features.Loyalty.Commands.EnrollCustomer;
using POSApi.Application.Features.Loyalty.Commands.ExpirePoints;
using POSApi.Application.Features.Loyalty.Commands.RedeemPoints;
using POSApi.Application.Features.Loyalty.Commands.RedeemReward;
using POSApi.Application.Features.Loyalty.Commands.UpdateCustomerTier;
using POSApi.Application.Features.Loyalty.Commands.UpdateLoyaltyProgram;
using POSApi.Application.Features.Loyalty.Commands.UpdateReward;
using POSApi.Application.Features.Loyalty.Queries.GetAllTiers;
using POSApi.Application.Features.Loyalty.Queries.GetAvailableRewards;
using POSApi.Application.Features.Loyalty.Queries.GetCustomerLoyalty;
using POSApi.Application.Features.Loyalty.Queries.GetCustomerRedemptions;
using POSApi.Application.Features.Loyalty.Queries.GetLoyaltyDashboard;
using POSApi.Application.Features.Loyalty.Queries.GetLoyaltyProgram;
using POSApi.Application.Features.Loyalty.Queries.GetLoyaltyTransactions;
using POSApi.Application.Features.Loyalty.Queries.GetPointsExpiring;
using POSApi.Domain.Entities;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoyaltyController : ControllerBase
{
    private readonly IMediator _mediator;

    public LoyaltyController(IMediator mediator)
    {
        _mediator = mediator;
    }

    #region Loyalty Program

    /// <summary>
    /// Get active loyalty program
    /// </summary>
    [HttpGet("program")]
    public async Task<ActionResult<LoyaltyProgramDto>> GetProgram(CancellationToken cancellationToken)
    {
        var query = new GetLoyaltyProgramQuery();
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound("No active loyalty program found");

        return Ok(result);
    }

    /// <summary>
    /// Create loyalty program (Owner only)
    /// </summary>
    [HttpPost("program")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult<Guid>> CreateProgram([FromBody] CreateLoyaltyProgramRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateLoyaltyProgramCommand
            {
                Name = request.Name,
                Description = request.Description,
                PointsPerDollar = request.PointsPerDollar,
                MinimumPurchaseAmount = request.MinimumPurchaseAmount,
                PointsExpiryDays = request.PointsExpiryDays
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetProgram), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update loyalty program (Owner only)
    /// </summary>
    [HttpPut("program/{id:guid}")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> UpdateProgram(Guid id, [FromBody] UpdateLoyaltyProgramRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateLoyaltyProgramCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                PointsPerDollar = request.PointsPerDollar,
                MinimumPurchaseAmount = request.MinimumPurchaseAmount,
                PointsExpiryDays = request.PointsExpiryDays,
                IsActive = request.IsActive
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Customer Tiers

    /// <summary>
    /// Get all customer tiers
    /// </summary>
    [HttpGet("tiers")]
    public async Task<ActionResult<List<CustomerTierDto>>> GetTiers(CancellationToken cancellationToken)
    {
        var query = new GetAllTiersQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create customer tier (Owner/Manager only)
    /// </summary>
    [HttpPost("tiers")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<Guid>> CreateTier([FromBody] CreateTierRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateCustomerTierCommand
            {
                Name = request.Name,
                MinSpend = request.MinSpend,
                MinPoints = request.MinPoints,
                BenefitMultiplier = request.BenefitMultiplier,
                DiscountPercentage = request.DiscountPercentage,
                Color = request.Color,
                SortOrder = request.SortOrder
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetTiers), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update customer tier (Owner/Manager only)
    /// </summary>
    [HttpPut("tiers/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> UpdateTier(Guid id, [FromBody] UpdateTierRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateCustomerTierCommand
            {
                Id = id,
                Name = request.Name,
                MinSpend = request.MinSpend,
                MinPoints = request.MinPoints,
                BenefitMultiplier = request.BenefitMultiplier,
                DiscountPercentage = request.DiscountPercentage,
                Color = request.Color,
                SortOrder = request.SortOrder
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Customer Loyalty

    /// <summary>
    /// Get customer loyalty profile
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<CustomerLoyaltyDto>> GetCustomerLoyalty(Guid customerId, CancellationToken cancellationToken)
    {
        var query = new GetCustomerLoyaltyQuery(customerId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound($"Customer loyalty profile not found for customer {customerId}");

        return Ok(result);
    }

    /// <summary>
    /// Enroll customer in loyalty program
    /// </summary>
    [HttpPost("customer/{customerId:guid}/enroll")]
    public async Task<ActionResult<Guid>> EnrollCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var command = new EnrollCustomerCommand { CustomerId = customerId };
            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetCustomerLoyalty), new { customerId }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get customer loyalty transaction history
    /// </summary>
    [HttpGet("customer/{customerId:guid}/transactions")]
    public async Task<ActionResult<PagedResult<LoyaltyTransactionDto>>> GetTransactions(
        Guid customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] LoyaltyTransactionType? transactionType = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLoyaltyTransactionsQuery
        {
            CustomerId = customerId,
            Page = page,
            PageSize = pageSize,
            TransactionType = transactionType,
            StartDate = startDate,
            EndDate = endDate
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get points expiring soon for customer
    /// </summary>
    [HttpGet("customer/{customerId:guid}/expiring")]
    public async Task<ActionResult<List<LoyaltyTransactionDto>>> GetPointsExpiring(
        Guid customerId,
        [FromQuery] int daysThreshold = 30,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPointsExpiringQuery(customerId, daysThreshold);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Points Operations

    /// <summary>
    /// Earn points (triggered by order completion)
    /// </summary>
    [HttpPost("earn")]
    public async Task<ActionResult<int>> EarnPoints([FromBody] EarnPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new EarnPointsCommand
            {
                CustomerId = request.CustomerId,
                OrderAmount = request.OrderAmount,
                OrderId = request.OrderId
            };

            var pointsEarned = await _mediator.Send(command, cancellationToken);
            return Ok(new { pointsEarned, message = $"Successfully earned {pointsEarned} points" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Redeem points
    /// </summary>
    [HttpPost("redeem")]
    public async Task<ActionResult> RedeemPoints([FromBody] RedeemPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RedeemPointsCommand
            {
                CustomerId = request.CustomerId,
                Points = request.Points,
                Description = request.Description,
                OrderId = request.OrderId
            };

            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = $"Successfully redeemed {request.Points} points" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Adjust points (Manager only)
    /// </summary>
    [HttpPost("adjust")]
    [Authorize(Roles = "Manager,Owner")]
    public async Task<ActionResult> AdjustPoints([FromBody] AdjustPointsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new AdjustPointsCommand
            {
                CustomerId = request.CustomerId,
                PointsAdjustment = request.PointsAdjustment,
                Reason = request.Reason
            };

            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = $"Successfully adjusted points by {request.PointsAdjustment}" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Expire points for all customers (Background job / Owner only)
    /// </summary>
    [HttpPost("expire")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> ExpirePoints(CancellationToken cancellationToken)
    {
        var command = new ExpirePointsCommand();
        var totalExpired = await _mediator.Send(command, cancellationToken);
        return Ok(new { totalPointsExpired = totalExpired, message = $"Expired {totalExpired} points across all customers" });
    }

    #endregion

    #region Rewards

    /// <summary>
    /// Get available rewards
    /// </summary>
    [HttpGet("rewards")]
    public async Task<ActionResult<List<RewardDto>>> GetRewards([FromQuery] Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        var query = new GetAvailableRewardsQuery { CustomerId = customerId };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create reward (Owner/Manager only)
    /// </summary>
    [HttpPost("rewards")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult<Guid>> CreateReward([FromBody] CreateRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateRewardCommand
            {
                Name = request.Name,
                Description = request.Description,
                PointsCost = request.PointsCost,
                RewardType = request.RewardType,
                Value = request.Value,
                ProductId = request.ProductId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                MaxRedemptionsPerCustomer = request.MaxRedemptionsPerCustomer,
                TotalRedemptionsAllowed = request.TotalRedemptionsAllowed
            };

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetRewards), new { id = result }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update reward (Owner/Manager only)
    /// </summary>
    [HttpPut("rewards/{id:guid}")]
    [Authorize(Roles = "Owner,Manager")]
    public async Task<ActionResult> UpdateReward(Guid id, [FromBody] UpdateRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateRewardCommand
            {
                Id = id,
                Name = request.Name,
                Description = request.Description,
                PointsCost = request.PointsCost,
                RewardType = request.RewardType,
                Value = request.Value,
                ProductId = request.ProductId,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                MaxRedemptionsPerCustomer = request.MaxRedemptionsPerCustomer,
                TotalRedemptionsAllowed = request.TotalRedemptionsAllowed,
                IsActive = request.IsActive
            };

            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Redeem reward
    /// </summary>
    [HttpPost("rewards/{rewardId:guid}/redeem")]
    public async Task<ActionResult<Guid>> RedeemReward(Guid rewardId, [FromBody] RedeemRewardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new RedeemRewardCommand
            {
                CustomerId = request.CustomerId,
                RewardId = rewardId
            };

            var redemptionId = await _mediator.Send(command, cancellationToken);
            return Ok(new { redemptionId, message = "Reward redeemed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get customer reward redemptions
    /// </summary>
    [HttpGet("customer/{customerId:guid}/redemptions")]
    public async Task<ActionResult<List<RewardRedemptionDto>>> GetRedemptions(
        Guid customerId,
        [FromQuery] bool? includeUsed = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerRedemptionsQuery(customerId) { IncludeUsed = includeUsed };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    #endregion

    #region Analytics

    /// <summary>
    /// Get loyalty program analytics dashboard (Manager/Owner only)
    /// </summary>
    [HttpGet("dashboard")]
    [Authorize(Roles = "Manager,Owner")]
    public async Task<ActionResult<LoyaltyDashboardDto>> GetDashboard(
        [FromQuery] int topCustomers = 10,
        [FromQuery] int monthsBack = 6,
        CancellationToken cancellationToken = default)
    {
        var query = new GetLoyaltyDashboardQuery
        {
            TopCustomersCount = topCustomers,
            MonthsBack = monthsBack
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    #endregion
}
