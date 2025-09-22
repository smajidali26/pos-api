using Microsoft.Extensions.Logging;
using POSApi.Domain.Events;
using POSApi.Infrastructure.Services;

namespace POSApi.Infrastructure.EventHandlers;

public class OrderCompletedHandler : IDomainEventHandler<OrderCompletedEvent>
{
    private readonly ILogger<OrderCompletedHandler> _logger;

    public OrderCompletedHandler(ILogger<OrderCompletedHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderCompletedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Order {OrderNumber} completed. Total amount: {TotalAmount:C}, Payment method: {PaymentMethod}",
            domainEvent.OrderNumber, domainEvent.TotalAmount, domainEvent.PaymentMethod);

        // Here you could implement:
        // - Print receipt
        // - Send receipt via email
        // - Update sales reports
        // - Process loyalty points
        // - Update inventory levels

        return Task.CompletedTask;
    }
}