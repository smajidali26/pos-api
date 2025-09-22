using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using POSApi.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace POSApi.Infrastructure.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
    bool ValidateToken(string token, out ClaimsPrincipal? principal);
    string? GetUserIdFromToken(string token);
    string? GetUsernameFromToken(string token);
    string? GetRoleFromToken(string token);
    DateTime GetTokenExpiration(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"] ?? "480");
    }

    public string GenerateToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("FullName", user.FullName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateToken(string token, out ClaimsPrincipal? principal)
    {
        principal = null;
        
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public string? GetUserIdFromToken(string token)
    {
        if (ValidateToken(token, out ClaimsPrincipal? principal))
        {
            return principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        return null;
    }

    public string? GetUsernameFromToken(string token)
    {
        if (ValidateToken(token, out ClaimsPrincipal? principal))
        {
            return principal?.FindFirst(ClaimTypes.Name)?.Value;
        }
        return null;
    }

    public string? GetRoleFromToken(string token)
    {
        if (ValidateToken(token, out ClaimsPrincipal? principal))
        {
            return principal?.FindFirst(ClaimTypes.Role)?.Value;
        }
        return null;
    }

    public DateTime GetTokenExpiration(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo;
        }
        catch
        {
            return DateTime.MinValue;
        }
    }
}