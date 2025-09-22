using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POSApi.Application.Common.DTOs;
using POSApi.Infrastructure.Services;
using System.Security.Claims;

namespace POSApi.Web.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthenticationService authenticationService,
        IJwtTokenService jwtTokenService,
        ICurrentUserService currentUserService,
        ILogger<AuthController> logger)
    {
        _authenticationService = authenticationService;
        _jwtTokenService = jwtTokenService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new LoginResponse 
            { 
                IsSuccess = false, 
                ErrorMessage = "Invalid request data" 
            });
        }

        try
        {
            var result = await _authenticationService.LoginAsync(request.Username, request.Password, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new LoginResponse 
                { 
                    IsSuccess = false, 
                    ErrorMessage = result.ErrorMessage 
                });
            }

            var response = new LoginResponse
            {
                IsSuccess = true,
                Token = result.Token,
                TokenExpiration = result.TokenExpiration,
                User = new UserInfo
                {
                    Id = result.User!.Id,
                    Username = result.User.Username,
                    FirstName = result.User.FirstName,
                    LastName = result.User.LastName,
                    FullName = result.User.FullName,
                    Email = result.User.Email,
                    Role = result.User.Role,
                    RoleName = result.User.Role.ToString(),
                    IsActive = result.User.IsActive,
                    LastLoginDate = result.User.LastLoginDate,
                    CreatedAt = result.User.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
            return StatusCode(500, new LoginResponse 
            { 
                IsSuccess = false, 
                ErrorMessage = "An internal error occurred" 
            });
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new RegisterResponse 
            { 
                IsSuccess = false, 
                ErrorMessage = "Invalid request data" 
            });
        }

        try
        {
            // Map Application DTO to Infrastructure DTO
            var registrationRequest = new UserRegistrationRequest
            {
                Username = request.Username,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            };

            var result = await _authenticationService.RegisterAsync(registrationRequest, cancellationToken);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new RegisterResponse 
                { 
                    IsSuccess = false, 
                    ErrorMessage = result.ErrorMessage 
                });
            }

            var response = new RegisterResponse
            {
                IsSuccess = true,
                Token = result.Token,
                TokenExpiration = result.TokenExpiration,
                User = new UserInfo
                {
                    Id = result.User!.Id,
                    Username = result.User.Username,
                    FirstName = result.User.FirstName,
                    LastName = result.User.LastName,
                    FullName = result.User.FullName,
                    Email = result.User.Email,
                    Role = result.User.Role,
                    RoleName = result.User.Role.ToString(),
                    IsActive = result.User.IsActive,
                    LastLoginDate = result.User.LastLoginDate,
                    CreatedAt = result.User.CreatedAt
                }
            };

            return CreatedAtAction(nameof(GetCurrentUser), response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return StatusCode(500, new RegisterResponse 
            { 
                IsSuccess = false, 
                ErrorMessage = "An internal error occurred" 
            });
        }
    }

    /// <summary>
    /// Logout current user
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        try
        {
            if (_currentUserService.UserId.HasValue)
            {
                await _authenticationService.LogoutAsync(_currentUserService.UserId.Value, cancellationToken);
            }

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", _currentUserService.UserId);
            return StatusCode(500, new { message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current user information
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfo>> GetCurrentUser(CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized();
            }

            var user = await _authenticationService.GetCurrentUserAsync(_currentUserService.UserId.Value, cancellationToken);
            
            if (user == null)
            {
                return NotFound();
            }

            var userInfo = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                RoleName = user.Role.ToString(),
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                CreatedAt = user.CreatedAt
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user: {UserId}", _currentUserService.UserId);
            return StatusCode(500, new { message = "An error occurred" });
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized();
            }

            var success = await _authenticationService.ChangePasswordAsync(
                _currentUserService.UserId.Value, 
                request.CurrentPassword, 
                request.NewPassword, 
                cancellationToken);

            if (!success)
            {
                return BadRequest(new { message = "Invalid current password or user not found" });
            }

            return Ok(new { message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", _currentUserService.UserId);
            return StatusCode(500, new { message = "An error occurred while changing password" });
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenValidationResponse>> ValidateToken([FromBody] TokenValidationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var isValid = await _authenticationService.ValidateTokenAsync(request.Token, cancellationToken);
            
            if (!isValid)
            {
                return Ok(new TokenValidationResponse 
                { 
                    IsValid = false, 
                    ErrorMessage = "Invalid or expired token" 
                });
            }

            var userId = _jwtTokenService.GetUserIdFromToken(request.Token);
            var tokenExpiration = _jwtTokenService.GetTokenExpiration(request.Token);
            
            UserInfo? userInfo = null;
            if (Guid.TryParse(userId, out var userGuid))
            {
                var user = await _authenticationService.GetCurrentUserAsync(userGuid, cancellationToken);
                if (user != null)
                {
                    userInfo = new UserInfo
                    {
                        Id = user.Id,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        FullName = user.FullName,
                        Email = user.Email,
                        Role = user.Role,
                        RoleName = user.Role.ToString(),
                        IsActive = user.IsActive,
                        LastLoginDate = user.LastLoginDate,
                        CreatedAt = user.CreatedAt
                    };
                }
            }

            return Ok(new TokenValidationResponse 
            { 
                IsValid = true, 
                User = userInfo,
                TokenExpiration = tokenExpiration
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new TokenValidationResponse 
            { 
                IsValid = false, 
                ErrorMessage = "An error occurred while validating token" 
            });
        }
    }

    /// <summary>
    /// Check if username is available
    /// </summary>
    [HttpPost("check-username")]
    [AllowAnonymous]
    public async Task<ActionResult<AvailabilityResponse>> CheckUsernameAvailability([FromBody] UsernameAvailabilityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var isAvailable = await _authenticationService.IsUsernameAvailableAsync(request.Username, cancellationToken);
            
            return Ok(new AvailabilityResponse 
            { 
                IsAvailable = isAvailable,
                Message = isAvailable ? "Username is available" : "Username is already taken"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", request.Username);
            return StatusCode(500, new AvailabilityResponse 
            { 
                IsAvailable = false, 
                Message = "An error occurred while checking username availability" 
            });
        }
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    [HttpPost("check-email")]
    [AllowAnonymous]
    public async Task<ActionResult<AvailabilityResponse>> CheckEmailAvailability([FromBody] EmailAvailabilityRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var isAvailable = await _authenticationService.IsEmailAvailableAsync(request.Email, cancellationToken);
            
            return Ok(new AvailabilityResponse 
            { 
                IsAvailable = isAvailable,
                Message = isAvailable ? "Email is available" : "Email is already registered"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", request.Email);
            return StatusCode(500, new AvailabilityResponse 
            { 
                IsAvailable = false, 
                Message = "An error occurred while checking email availability" 
            });
        }
    }

    /// <summary>
    /// Refresh JWT token (extend expiration)
    /// </summary>
    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<ActionResult<LoginResponse>> RefreshToken(CancellationToken cancellationToken)
    {
        try
        {
            if (!_currentUserService.UserId.HasValue)
            {
                return Unauthorized();
            }

            var user = await _authenticationService.GetCurrentUserAsync(_currentUserService.UserId.Value, cancellationToken);
            
            if (user == null || !user.IsActive)
            {
                return Unauthorized();
            }

            // Generate new token
            var newToken = _jwtTokenService.GenerateToken(user);
            var tokenExpiration = _jwtTokenService.GetTokenExpiration(newToken);

            var response = new LoginResponse
            {
                IsSuccess = true,
                Token = newToken,
                TokenExpiration = tokenExpiration,
                User = new UserInfo
                {
                    Id = user.Id,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    RoleName = user.Role.ToString(),
                    IsActive = user.IsActive,
                    LastLoginDate = user.LastLoginDate,
                    CreatedAt = user.CreatedAt
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for user: {UserId}", _currentUserService.UserId);
            return StatusCode(500, new LoginResponse 
            { 
                IsSuccess = false, 
                ErrorMessage = "An error occurred while refreshing token" 
            });
        }
    }
}