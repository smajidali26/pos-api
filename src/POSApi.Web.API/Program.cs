using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using POSApi.Application;
using POSApi.Infrastructure;
using POSApi.Infrastructure.Persistence;
using POSApi.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

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

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://yourfrontendapp.com") // Replace with your frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
            // In development, use EnsureCreated for simplicity
            // This will create the database with all current model entities
            logger.LogInformation("Development environment: Creating database schema from current model...");
            
            // Delete and recreate database to ensure it matches current model
            await context.Database.EnsureDeletedAsync();
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

app.Run();
