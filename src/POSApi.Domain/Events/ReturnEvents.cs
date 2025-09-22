using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class ReturnCreatedEvent : DomainEvent
{
    public Guid ReturnId { get; }
    public string ReturnNumber { get; }
    public Guid OriginalOrderId { get; }

    public ReturnCreatedEvent(Guid returnId, string returnNumber, Guid originalOrderId)
    {
        ReturnId = returnId;
        ReturnNumber = returnNumber;
        OriginalOrderId = originalOrderId;
    }
}

public class ReturnProcessedEvent : DomainEvent
{
    public Guid ReturnId { get; }
    public string ReturnNumber { get; }
    public decimal RefundAmount { get; }
    public RefundMethod RefundMethod { get; }

    public ReturnProcessedEvent(Guid returnId, string returnNumber, decimal refundAmount, RefundMethod refundMethod)
    {
        ReturnId = returnId;
        ReturnNumber = returnNumber;
        RefundAmount = refundAmount;
        RefundMethod = refundMethod;
    }
}

public class ReturnCompletedEvent : DomainEvent
{
    public Guid ReturnId { get; }
    public string ReturnNumber { get; }
    public decimal RefundAmount { get; }

    public ReturnCompletedEvent(Guid returnId, string returnNumber, decimal refundAmount)
    {
        ReturnId = returnId;
        ReturnNumber = returnNumber;
        RefundAmount = refundAmount;
    }
}

public class ReturnCancelledEvent : DomainEvent
{
    public Guid ReturnId { get; }
    public string ReturnNumber { get; }
    public string Reason { get; }

    public ReturnCancelledEvent(Guid returnId, string returnNumber, string reason)
    {
        ReturnId = returnId;
        ReturnNumber = returnNumber;
        Reason = reason;
    }
}