using Microsoft.AspNetCore.Authorization;

namespace POSApi.Web.API.Attributes;

/// <summary>
/// Requires the user to be authenticated and have Manager, Administrator, or Owner role
/// </summary>
public class RequireManagerAttribute : AuthorizeAttribute
{
    public RequireManagerAttribute() : base("RequireManager")
    {
    }
}

/// <summary>
/// Requires the user to be authenticated and have Administrator or Owner role
/// </summary>
public class RequireAdministratorAttribute : AuthorizeAttribute
{
    public RequireAdministratorAttribute() : base("RequireAdministrator")
    {
    }
}

/// <summary>
/// Requires the user to be authenticated and have Owner role
/// </summary>
public class RequireOwnerAttribute : AuthorizeAttribute
{
    public RequireOwnerAttribute() : base("RequireOwner")
    {
    }
}

/// <summary>
/// Requires the user to be authenticated with any valid role
/// </summary>
public class RequireAuthenticatedUserAttribute : AuthorizeAttribute
{
    public RequireAuthenticatedUserAttribute()
    {
        Roles = "Cashier,Manager,Administrator,Owner";
    }
}