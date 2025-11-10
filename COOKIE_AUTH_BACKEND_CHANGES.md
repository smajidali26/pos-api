# Backend Cookie Authentication Implementation

## Summary

The ASP.NET Core backend has been successfully updated to support **httpOnly cookie-based authentication** instead of relying solely on JWT tokens in the Authorization header.

## Changes Made

### 1. CORS Configuration (Program.cs:76-94)

**Updated to allow credentials for httpOnly cookies:**

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for httpOnly cookies
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourfrontendapp.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for httpOnly cookies
    });
});
```

**Key Change:** Added `.AllowCredentials()` to both Development and Production policies.

---

### 2. JWT Authentication Middleware (DependencyInjection.cs:71-110)

**Added `OnMessageReceived` event to read tokens from cookies:**

```csharp
options.Events = new JwtBearerEvents
{
    // Read JWT token from httpOnly cookie instead of Authorization header
    OnMessageReceived = context =>
    {
        // First check Authorization header (for backward compatibility)
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
        {
            context.Token = authHeader.Substring("Bearer ".Length).Trim();
        }
        // If no Authorization header, check cookie
        else if (context.Request.Cookies.TryGetValue("authToken", out var token))
        {
            context.Token = token;
        }
        return Task.CompletedTask;
    },
    // ... other events
};
```

**Key Change:** JWT middleware now reads tokens from `authToken` cookie if no Authorization header is present.

---

### 3. Login Endpoint (AuthController.cs:59-103)

**Sets httpOnly cookies on successful login:**

```csharp
// Set httpOnly cookies for secure token storage
var cookieOptions = new CookieOptions
{
    HttpOnly = true,  // Cannot be accessed by JavaScript (XSS protection)
    Secure = true,    // Only sent over HTTPS
    SameSite = SameSiteMode.Strict, // CSRF protection
    Expires = result.TokenExpiration
};

Response.Cookies.Append("authToken", result.Token, cookieOptions);

var refreshCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddDays(7)
};
Response.Cookies.Append("refreshToken", result.Token, refreshCookieOptions);
```

**Key Changes:**
- Sets `authToken` cookie with token expiration matching JWT
- Sets `refreshToken` cookie with 7-day expiration
- All cookies use `HttpOnly`, `Secure`, and `SameSite=Strict` for security

---

### 4. Logout Endpoint (AuthController.cs:193-215)

**Clears httpOnly cookies on logout:**

```csharp
[HttpPost("logout")]
[Authorize]
public async Task<ActionResult> Logout(CancellationToken cancellationToken)
{
    // ... existing logout logic ...

    // Clear httpOnly cookies
    Response.Cookies.Delete("authToken");
    Response.Cookies.Delete("refreshToken");

    return Ok(new { message = "Logged out successfully" });
}
```

**Key Change:** Deletes both `authToken` and `refreshToken` cookies.

---

### 5. Token Refresh Endpoint (AuthController.cs:424-497)

**Updates cookies with new tokens:**

```csharp
// Generate new token
var newToken = _jwtTokenService.GenerateToken(user);
var tokenExpiration = _jwtTokenService.GetTokenExpiration(newToken);

// Update httpOnly cookies with new token
var cookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = tokenExpiration
};

Response.Cookies.Append("authToken", newToken, cookieOptions);

var refreshCookieOptions = new CookieOptions
{
    HttpOnly = true,
    Secure = true,
    SameSite = SameSiteMode.Strict,
    Expires = DateTimeOffset.UtcNow.AddDays(7)
};
Response.Cookies.Append("refreshToken", newToken, refreshCookieOptions);
```

**Key Change:** Updates both cookies with fresh tokens on refresh.

---

### 6. Validate Token Endpoint (AuthController.cs:301-383)

**Supports token validation from cookies:**

```csharp
[HttpPost("validate-token")]
[AllowAnonymous]
public async Task<ActionResult<TokenValidationResponse>> ValidateToken(
    [FromBody] TokenValidationRequest? request,
    CancellationToken cancellationToken)
{
    // Try to get token from cookie first, then from request body
    string? token = null;
    if (Request.Cookies.TryGetValue("authToken", out var cookieToken))
    {
        token = cookieToken;
    }
    else if (request?.Token != null)
    {
        token = request.Token;
    }

    if (string.IsNullOrEmpty(token))
    {
        return Ok(new TokenValidationResponse
        {
            IsValid = false,
            ErrorMessage = "No token provided"
        });
    }

    // ... validation logic ...
}
```

**Key Change:** Reads token from cookie first, falls back to request body for backward compatibility.

---

## Cookie Configuration

### Cookie Names
- `authToken` - Main JWT authentication token
- `refreshToken` - Long-lived refresh token

### Cookie Settings

| Setting | Value | Purpose |
|---------|-------|---------|
| `HttpOnly` | `true` | Prevents JavaScript access (XSS protection) |
| `Secure` | `true` | Only sent over HTTPS (production security) |
| `SameSite` | `Strict` | Prevents CSRF attacks |
| `authToken Expires` | Token expiration (8 hours) | Matches JWT lifetime |
| `refreshToken Expires` | 7 days | Longer-lived for session persistence |

---

## Backward Compatibility

All endpoints maintain **backward compatibility** with Authorization header authentication:

1. **JWT Middleware** checks Authorization header first, then cookies
2. **Login/Refresh** still return tokens in response body (for migration period)
3. **ValidateToken** accepts tokens from both cookie and request body

This allows gradual migration and supports both authentication methods simultaneously.

---

## Security Features

### ✅ XSS Protection
- Cookies are `HttpOnly` - cannot be accessed by JavaScript
- Even if XSS vulnerability exists, tokens cannot be stolen

### ✅ CSRF Protection
- Cookies use `SameSite=Strict` flag
- Prevents cross-site request forgery attacks

### ✅ HTTPS Enforcement
- `Secure=true` ensures cookies only sent over encrypted connections
- Prevents man-in-the-middle attacks

### ✅ Automatic Token Management
- Cookies sent automatically with every request
- No manual header manipulation required

---

## Testing

### Development Environment

1. **Frontend URL**: `http://localhost:5173` (Vite dev server)
2. **Backend URL**: Configured in `appsettings.json`
3. **CORS**: Development policy allows localhost origins with credentials

### Test Checklist

- [ ] Login sets `authToken` and `refreshToken` cookies
- [ ] Cookies visible in browser DevTools (Application → Cookies)
- [ ] Cookies have `HttpOnly`, `Secure`, `SameSite` flags
- [ ] Protected endpoints work without Authorization header
- [ ] Token refresh updates cookies
- [ ] Logout clears cookies
- [ ] Session persists after page refresh
- [ ] 401 errors trigger automatic token refresh

---

## Production Deployment

### Configuration Changes Required

1. **Update CORS origin** in `Program.cs`:
   ```csharp
   policy.WithOrigins("https://yourfrontendapp.com")
   ```

2. **Ensure HTTPS** is enabled on your hosting platform

3. **Verify `Secure` flag** is set to `true` in production

4. **Configure environment variables** for sensitive settings

---

## Monitoring and Logging

All authentication operations are logged:
- Login attempts
- Token refresh operations
- Logout events
- Validation failures

Check logs for:
```
_logger.LogError(ex, "Error during login for username: {Username}", request.Username);
_logger.LogError(ex, "Error refreshing token for user: {UserId}", _currentUserService.UserId);
_logger.LogError(ex, "Error during logout for user: {UserId}", _currentUserService.UserId);
```

---

## Troubleshooting

### Cookies Not Being Set

**Problem**: Login successful but no cookies in browser

**Solutions**:
1. Verify CORS policy includes `.AllowCredentials()`
2. Check frontend is sending `withCredentials: true`
3. Ensure frontend and backend URLs match CORS configuration
4. Verify HTTPS in production (Secure flag requires it)

### 401 Errors on Protected Routes

**Problem**: All protected endpoints return 401

**Solutions**:
1. Check if cookies are being sent (DevTools → Network → Request Headers)
2. Verify JWT middleware `OnMessageReceived` event is configured
3. Check cookie names match (`authToken`)
4. Ensure token hasn't expired

### CORS Errors

**Problem**: Browser blocks requests with CORS error

**Solutions**:
1. Verify CORS policy includes frontend origin
2. Add `.AllowCredentials()` to CORS policy
3. Don't use wildcard `*` with credentials
4. Check request includes `withCredentials: true`

---

## Files Modified

| File | Purpose | Changes |
|------|---------|---------|
| `Program.cs` | CORS configuration | Added `.AllowCredentials()` |
| `DependencyInjection.cs` | JWT middleware | Added `OnMessageReceived` event |
| `AuthController.cs` | Login endpoint | Sets httpOnly cookies |
| `AuthController.cs` | Logout endpoint | Deletes cookies |
| `AuthController.cs` | Refresh endpoint | Updates cookies |
| `AuthController.cs` | Validate endpoint | Reads from cookies |

---

## Next Steps

1. ✅ Backend updated for cookie authentication
2. ✅ Frontend updated to use `withCredentials`
3. ⏳ Test end-to-end authentication flow
4. ⏳ Deploy to staging environment
5. ⏳ Update production CORS configuration
6. ⏳ Monitor authentication logs

---

## Support

For issues or questions:
1. Check browser DevTools (Console, Network, Application tabs)
2. Review backend logs for authentication errors
3. Verify cookie settings match documentation
4. Ensure CORS configuration is correct
