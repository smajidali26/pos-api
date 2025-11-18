using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using POSApi.Application;
using POSApi.Infrastructure;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services;
using POSApi.Infrastructure.BackgroundJobs;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as strings instead of numbers
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "POS System API", 
        Version = "v1",
        Description = "A comprehensive Point of Sale (POS) system API with authentication, inventory management, and sales tracking.",
        Contact = new OpenApiContact
        {
            Name = "POS System Support",
            Email = "support@pos.com"
        }
    });
    
    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    
    // Add XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Group endpoints by tags
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Response Caching and Memory Cache
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache();

// Add Hangfire for background jobs
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add the Hangfire server
builder.Services.AddHangfireServer();

// Add CORS for development with credentials support (required for httpOnly cookies)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174", "http://localhost:3000") // Frontend dev URLs
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for httpOnly cookies
    });

    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourfrontendapp.com") // Replace with your frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for httpOnly cookies
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POS System API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the root
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
        c.EnableFilter();
        c.ShowExtensions();
        c.EnableValidator();
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);
    });
    
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

app.UseHttpsRedirection();

// Response Caching (must be before Authentication)
app.UseResponseCaching();

// Hangfire Dashboard (only in development for security)
if (app.Environment.IsDevelopment())
{
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = new[] { new HangfireAuthorizationFilter() }
    });
}

// Authentication & Authorization (Order is important!)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database initialization and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<PosDbContext>();
        
        logger.LogInformation("Initializing database...");
        
        if (app.Environment.IsDevelopment())
        {
            // In development, drop and recreate database to apply schema changes
            logger.LogInformation("Development environment: Recreating database with latest schema...");

            var databaseExists = await context.Database.CanConnectAsync();

            if (databaseExists)
            {
                logger.LogInformation("Dropping existing database to apply schema changes...");
                await context.Database.EnsureDeletedAsync();
                logger.LogInformation("Database dropped successfully");
            }

            logger.LogInformation("Creating database schema from current model...");
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("Database schema created successfully from current model");
        }
        else
        {
            // In production, use migrations
            logger.LogInformation("Production environment: Checking for pending migrations...");
            
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
            
            logger.LogInformation("Applied migrations: {AppliedCount}", appliedMigrations.Count());
            logger.LogInformation("Pending migrations: {PendingCount}", pendingMigrations.Count());
            
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying pending migrations: {Migrations}", string.Join(", ", pendingMigrations));
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrations applied successfully");
            }
            else
            {
                logger.LogInformation("Database is up to date");
            }
        }

        // Seed data
        logger.LogInformation("Starting data seeding...");
        
        var dataSeeder = services.GetRequiredService<DataSeeder>();
        await dataSeeder.SeedAsync();
        
        logger.LogInformation("Database seeding completed successfully");
        
        // Log default user credentials for development
        if (app.Environment.IsDevelopment())
        {
            logger.LogInformation("=== DEFAULT USERS FOR DEVELOPMENT ===");
            logger.LogInformation("Owner: admin / Admin@123");
            logger.LogInformation("Manager: manager / Manager@123");
            logger.LogInformation("Cashier 1: cashier1 / Cashier@123");
            logger.LogInformation("Cashier 2: cashier2 / Cashier@123");
            logger.LogInformation("=======================================");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database");
        
        if (app.Environment.IsDevelopment())
        {
            logger.LogError("Database initialization failed. Please check your connection string and ensure SQL Server is running.");
            logger.LogError("Connection string: {ConnectionString}", 
                builder.Configuration.GetConnectionString("DefaultConnection"));
        }
        
        throw;
    }
}

// Schedule recurring background jobs
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Scheduling recurring background jobs...");

    // Analytics Jobs
    recurringJobManager.AddOrUpdate<IAnalyticsBackgroundJobs>(
        "daily-sales-forecasts",
        job => job.GenerateDailySalesForecastsAsync(),
        "0 2 * * *", // Daily at 2 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<IAnalyticsBackgroundJobs>(
        "monthly-abc-classification",
        job => job.RecalculateABCClassificationAsync(),
        "0 3 1 * *", // Monthly on 1st at 3 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<IAnalyticsBackgroundJobs>(
        "monthly-inventory-turnover",
        job => job.CalculateInventoryTurnoverAsync(),
        "0 4 1 * *", // Monthly on 1st at 4 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<IAnalyticsBackgroundJobs>(
        "weekly-forecast-cleanup",
        job => job.CleanupOldForecastsAsync(),
        "0 1 * * 0", // Weekly on Sunday at 1 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    // Loyalty Jobs
    recurringJobManager.AddOrUpdate<ILoyaltyBackgroundJobs>(
        "daily-points-expiry",
        job => job.ExpirePointsAsync(),
        "0 1 * * *", // Daily at 1 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<ILoyaltyBackgroundJobs>(
        "weekly-expiry-notifications",
        job => job.SendPointsExpiryNotificationsAsync(),
        "0 9 * * 1", // Weekly on Monday at 9 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<ILoyaltyBackgroundJobs>(
        "daily-tier-updates",
        job => job.UpdateCustomerTiersAsync(),
        "0 5 * * *", // Daily at 5 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    recurringJobManager.AddOrUpdate<ILoyaltyBackgroundJobs>(
        "monthly-loyalty-report",
        job => job.GenerateMonthlyLoyaltyReportAsync(),
        "0 6 1 * *", // Monthly on 1st at 6 AM
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });

    logger.LogInformation("Background jobs scheduled successfully");
    logger.LogInformation("Hangfire Dashboard available at: /hangfire (Development only)");
}

app.Run();

// Hangfire Authorization Filter for Development
public class HangfireAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Allow all access in development
        // In production, implement proper authorization
        return true;
    }
}
