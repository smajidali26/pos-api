using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class OrderCreatedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public Guid CashierId { get; }

    public OrderCreatedEvent(Guid orderId, string orderNumber, Guid cashierId)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        CashierId = cashierId;
    }
}