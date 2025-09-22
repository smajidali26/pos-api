using Microsoft.Extensions.Logging;
using POSApi.Domain.Events;
using POSApi.Infrastructure.Services;

namespace POSApi.Infrastructure.EventHandlers;

public class LowStockAlertHandler : IDomainEventHandler<LowStockAlertEvent>
{
    private readonly ILogger<LowStockAlertHandler> _logger;

    public LowStockAlertHandler(ILogger<LowStockAlertHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(LowStockAlertEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Low stock alert for product {ProductName} (ID: {ProductId}). Current stock: {CurrentStock}, Min level: {MinLevel}",
            domainEvent.ProductName, domainEvent.ProductId, domainEvent.CurrentStock, domainEvent.MinStockLevel);

        // Here you could implement:
        // - Send email notifications to managers
        // - Create automatic purchase orders
        // - Update dashboard alerts
        // - Send SMS notifications

        return Task.CompletedTask;
    }
}