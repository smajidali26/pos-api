using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace POSApi.Infrastructure.Services;

public interface IAuthenticationService
{
    Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    Task<User?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticationService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthenticationResult> LoginAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByUsernameAsync(username, cancellationToken);
            
            if (user == null || !user.IsActive)
            {
                _logger.LogWarning("Login attempt failed for username: {Username}", username);
                return AuthenticationResult.Failure("Invalid username or password");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid password attempt for username: {Username}", username);
                return AuthenticationResult.Failure("Invalid username or password");
            }

            // Update last login date
            user.RecordLogin();
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);
            var expiration = _jwtTokenService.GetTokenExpiration(token);

            _logger.LogInformation("User {Username} logged in successfully", username);
            
            return AuthenticationResult.Success(token, user, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for username: {Username}", username);
            return AuthenticationResult.Failure("An error occurred during login");
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(UserRegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if username is available
            if (!await IsUsernameAvailableAsync(request.Username, cancellationToken))
            {
                return AuthenticationResult.Failure("Username is already taken");
            }

            // Check if email is available
            if (!await IsEmailAvailableAsync(request.Email, cancellationToken))
            {
                return AuthenticationResult.Failure("Email is already registered");
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User(
                request.Username,
                request.FirstName,
                request.LastName,
                request.Email,
                passwordHash,
                request.Role);

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Generate JWT token
            var token = _jwtTokenService.GenerateToken(user);
            var expiration = _jwtTokenService.GetTokenExpiration(token);

            _logger.LogInformation("User {Username} registered successfully", request.Username);
            
            return AuthenticationResult.Success(token, user, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for username: {Username}", request.Username);
            return AuthenticationResult.Failure("An error occurred during registration");
        }
    }

    public async Task<bool> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                // In a more sophisticated implementation, you might want to:
                // 1. Blacklist the token
                // 2. Store logout time
                // 3. Clear refresh tokens
                
                _logger.LogInformation("User {UserId} logged out", userId);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            
            if (user == null || !user.IsActive)
            {
                return false;
            }

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                return false;
            }

            // Update password
            var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatePassword(newPasswordHash);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Password changed for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<User?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user: {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_jwtTokenService.ValidateToken(token, out var principal))
            {
                var userIdClaim = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
                    return user != null && user.IsActive;
                }
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return false;
        }
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _unitOfWork.Users.GetByUsernameAsync(username, cancellationToken);
            return existingUser == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username availability: {Username}", username);
            return false;
        }
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var existingUser = await _unitOfWork.Users.GetByEmailAsync(email, cancellationToken);
            return existingUser == null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email availability: {Email}", email);
            return false;
        }
    }
}

// Infrastructure DTOs - Different name to avoid conflicts
public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Cashier;
}

public class AuthenticationResult
{
    public bool IsSuccess { get; private set; }
    public string? Token { get; private set; }
    public User? User { get; private set; }
    public DateTime? TokenExpiration { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AuthenticationResult(bool isSuccess, string? token = null, User? user = null, 
                               DateTime? tokenExpiration = null, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        Token = token;
        User = user;
        TokenExpiration = tokenExpiration;
        ErrorMessage = errorMessage;
    }

    public static AuthenticationResult Success(string token, User user, DateTime tokenExpiration)
    {
        return new AuthenticationResult(true, token, user, tokenExpiration);
    }

    public static AuthenticationResult Failure(string errorMessage)
    {
        return new AuthenticationResult(false, errorMessage: errorMessage);
    }
}