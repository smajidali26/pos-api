using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs.Requests;
using POSApi.Application.Features.Users.Commands.AdminResetPassword;
using POSApi.Application.Features.Users.Commands.ChangePassword;
using POSApi.Application.Features.Users.Commands.CreateUser;
using POSApi.Application.Features.Users.Commands.ToggleUserStatus;
using POSApi.Application.Features.Users.Queries.GetAllUsers;
using POSApi.Infrastructure.Services;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Change password for current user
    /// </summary>
    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Set userId from current authenticated user
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized("User ID not found in token");
            }

            command.UserId = _currentUserService.UserId.Value;

            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Password changed successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get all users (Owner only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _mediator.Send(new GetAllUsersQuery(), cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Create new user (Owner only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> CreateUser([FromBody] CreateUserCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetAllUsers), new { id = userId }, new { id = userId, message = "User created successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Toggle user active status (Owner only)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> ToggleUserStatus(Guid id, [FromBody] ToggleStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new ToggleUserStatusCommand
            {
                UserId = id,
                IsActive = request.IsActive
            };

            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = $"User {(request.IsActive ? "activated" : "deactivated")} successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Admin reset password (Owner only - no current password required)
    /// </summary>
    [HttpPost("reset-password")]
    [Authorize(Roles = "Owner")]
    public async Task<ActionResult> AdminResetPassword([FromBody] AdminResetPasswordCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await _mediator.Send(command, cancellationToken);
            return Ok(new { message = "Password reset successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
