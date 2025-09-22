using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public string Reason { get; }

    public OrderCancelledEvent(Guid orderId, string orderNumber, string reason)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        Reason = reason;
    }
}