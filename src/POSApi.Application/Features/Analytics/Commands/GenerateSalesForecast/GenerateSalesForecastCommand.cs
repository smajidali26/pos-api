using FluentValidation;
using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Application.Common.Services;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Analytics.Commands.GenerateSalesForecast;

public class GenerateSalesForecastCommand : ICommand<bool>
{
    public List<Guid>? ProductIds { get; set; } // If null, forecast for all products
    public Guid? StoreId { get; set; }
    public int ForecastDays { get; set; } = 30;
}

public class GenerateSalesForecastCommandValidator : AbstractValidator<GenerateSalesForecastCommand>
{
    public GenerateSalesForecastCommandValidator()
    {
        RuleFor(x => x.ForecastDays)
            .GreaterThan(0).WithMessage("Forecast days must be greater than 0")
            .LessThanOrEqualTo(365).WithMessage("Forecast days cannot exceed 365");
    }
}

public class GenerateSalesForecastCommandHandler : ICommandHandler<GenerateSalesForecastCommand, bool>
{
    private readonly ISalesForecastingService _forecastingService;
    private readonly IUnitOfWork _unitOfWork;

    public GenerateSalesForecastCommandHandler(ISalesForecastingService forecastingService, IUnitOfWork unitOfWork)
    {
        _forecastingService = forecastingService;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(GenerateSalesForecastCommand request, CancellationToken cancellationToken)
    {
        // Get products to forecast
        var productsQuery = _unitOfWork.Context.Set<Product>().Where(p => p.IsActive);

        if (request.ProductIds != null && request.ProductIds.Any())
        {
            productsQuery = productsQuery.Where(p => request.ProductIds.Contains(p.Id));
        }

        var products = await productsQuery.ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            try
            {
                // Use auto-select method for best forecast
                var (method, forecasts) = await _forecastingService.SelectBestMethodAndForecast(
                    product.Id, request.StoreId, request.ForecastDays, cancellationToken);

                // Delete existing forecasts for this product/store combination
                var existingForecasts = await _unitOfWork.Context.Set<SalesForecast>()
                    .Where(sf => sf.ProductId == product.Id && sf.StoreId == request.StoreId)
                    .ToListAsync(cancellationToken);

                _unitOfWork.Context.Set<SalesForecast>().RemoveRange(existingForecasts);

                // Save new forecasts
                foreach (var forecast in forecasts)
                {
                    var forecastMethod = Enum.Parse<ForecastMethod>(method, true);

                    var salesForecast = new SalesForecast(
                        forecast.Date,
                        product.Id,
                        request.StoreId,
                        forecast.PredictedQuantity,
                        forecast.PredictedRevenue,
                        0.8m, // Default confidence level
                        forecastMethod
                    );

                    _unitOfWork.Context.Set<SalesForecast>().Add(salesForecast);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue with other products
                Console.WriteLine($"Error forecasting for product {product.Id}: {ex.Message}");
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
