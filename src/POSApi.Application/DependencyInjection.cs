using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using POSApi.Application.Common.Mappings;
using POSApi.Application.Common.Services;

namespace POSApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));

        // MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        // FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Advanced Analytics Services
        services.AddScoped<ISalesForecastingService, SalesForecastingService>();
        services.AddScoped<IABCAnalysisService, ABCAnalysisService>();
        services.AddScoped<IInventoryTurnoverService, InventoryTurnoverService>();

        return services;
    }
}