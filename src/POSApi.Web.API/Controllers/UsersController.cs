using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Features.Users.Commands.ChangePassword;
using POSApi.Application.Features.Users.Commands.CreateUser;
using POSApi.Application.Features.Users.Commands.AdminResetPassword;
using POSApi.Infrastructure.Services;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
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
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        var userDtos = users.Select(u => new
        {
            id = u.Id,
            username = u.Username,
            firstName = u.FirstName,
            lastName = u.LastName,
            fullName = u.FullName,
            email = u.Email,
            role = (int)u.Role,
            roleName = u.Role.ToString(),
            isActive = u.IsActive,
            lastLoginDate = u.LastLoginDate,
            createdAt = u.CreatedAt
        });

        return Ok(userDtos);
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
            var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (request.IsActive)
            {
                user.Activate();
            }
            else
            {
                user.Deactivate();
            }

            await _unitOfWork.Users.UpdateAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Ok(new { message = $"User {(request.IsActive ? "activated" : "deactivated")} successfully" });
        }
        catch (Exception ex)
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

public class ToggleStatusRequest
{
    public bool IsActive { get; set; }
}
