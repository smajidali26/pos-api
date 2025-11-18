using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using POSApi.Domain.Events;
using POSApi.Domain.Repositories;
using POSApi.Infrastructure.EventHandlers;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Repositories;
using POSApi.Infrastructure.Repositories.Interfaces;
using POSApi.Infrastructure.Services;
using POSApi.Infrastructure.Services.Interfaces;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace POSApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Get environment
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var isDevelopment = environment == "Development";

        // Database
        services.AddDbContext<PosDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            
            // In development, suppress the pending model changes warning
            // since we're using EnsureCreated instead of migrations
            if (isDevelopment)
            {
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
            }
        });

        // JWT Authentication
        var jwtSettings = configuration.GetSection("Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // Set to true in production
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true
            };

            // Custom events for cookie-based authentication
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
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.StatusCode = 401;
                    context.Response.ContentType = "application/json";
                    var result = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        error = "You are not authorized to access this resource",
                        message = "Please provide a valid JWT token"
                    });
                    return context.Response.WriteAsync(result);
                }
            };
        });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireManager", policy =>
                policy.RequireRole("Manager", "Administrator", "Owner"));
            
            options.AddPolicy("RequireAdministrator", policy =>
                policy.RequireRole("Administrator", "Owner"));
            
            options.AddPolicy("RequireOwner", policy =>
                policy.RequireRole("Owner"));
        });

        // Add HttpContextAccessor for CurrentUserService
        services.AddHttpContextAccessor();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ISizeRepository, SizeRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
        services.AddScoped<IReturnRepository, ReturnRepository>();
        services.AddScoped<IPromotionRepository, PromotionRepository>();

        // Unit of Measure repositories
        services.AddScoped<IUnitTypeRepository, UnitTypeRepository>();
        services.AddScoped<IUnitOfMeasureRepository, UnitOfMeasureRepository>();
        services.AddScoped<IProductUnitRepository, ProductUnitRepository>();

        // Multi-Store Management repositories
        services.AddScoped<IStoreRepository, StoreRepository>();
        // TODO: Uncomment when StoreInventory entity is created
        // services.AddScoped<IStoreInventoryRepository, StoreInventoryRepository>();
        // TODO: Uncomment when InterStoreTransfer entity is created
        // services.AddScoped<IInterStoreTransferRepository, InterStoreTransferRepository>();

        // Loyalty Program repositories
        services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
        services.AddScoped<ICustomerTierRepository, CustomerTierRepository>();
        services.AddScoped<ICustomerLoyaltyRepository, CustomerLoyaltyRepository>();
        services.AddScoped<IRewardRepository, RewardRepository>();
// Employee Management repositories        services.AddScoped<IEmployeeProfileRepository, EmployeeProfileRepository>();        services.AddScoped<IShiftRepository, ShiftRepository>();        services.AddScoped<IShiftAttendanceRepository, ShiftAttendanceRepository>();        services.AddScoped<ICommissionRepository, CommissionRepository>();        services.AddScoped<ICommissionTransactionRepository, CommissionTransactionRepository>();        services.AddScoped<IPerformanceMetricRepository, PerformanceMetricRepository>();

        // Authentication Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        // Business Services
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IOrderNumberGenerator, OrderNumberGenerator>();
        services.AddScoped<IReceiptService, ReceiptService>();
        services.AddScoped<IInfrastructureReportService, InfrastructureReportService>();
        services.AddScoped<Services.DataSeeder>();

        // Payment Services
        services.AddScoped<IPaymentGatewayService, StripePaymentService>();

        // Domain Event Handlers
        services.AddScoped<IDomainEventHandler<LowStockAlertEvent>, LowStockAlertHandler>();
        services.AddScoped<IDomainEventHandler<OrderCompletedEvent>, OrderCompletedHandler>();

        // Background Jobs
        // services.AddScoped<BackgroundJobs.IAnalyticsBackgroundJobs, BackgroundJobs.AnalyticsBackgroundJobs>();
        // services.AddScoped<BackgroundJobs.IEmployeeBackgroundJobs, BackgroundJobs.EmployeeBackgroundJobs>();
        services.AddScoped<BackgroundJobs.ILoyaltyBackgroundJobs, BackgroundJobs.LoyaltyBackgroundJobs>();

        // Export Services
        services.AddScoped<Services.Export.IExportService, Services.Export.PdfExportService>();
        services.AddScoped<Services.Export.CsvExportService>();

        return services;
    }
}