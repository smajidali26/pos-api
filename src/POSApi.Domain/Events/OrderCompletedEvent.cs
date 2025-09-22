using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class OrderCompletedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string OrderNumber { get; }
    public decimal TotalAmount { get; }
    public PaymentMethod PaymentMethod { get; }

    public OrderCompletedEvent(Guid orderId, string orderNumber, decimal totalAmount, PaymentMethod paymentMethod)
    {
        OrderId = orderId;
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
        PaymentMethod = paymentMethod;
    }
}