using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class PurchaseOrderCreatedEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public Guid VendorId { get; }

    public PurchaseOrderCreatedEvent(Guid purchaseOrderId, string orderNumber, Guid vendorId)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        VendorId = vendorId;
    }
}

public class PurchaseOrderSubmittedEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public Guid VendorId { get; }
    public decimal TotalAmount { get; }

    public PurchaseOrderSubmittedEvent(Guid purchaseOrderId, string orderNumber, Guid vendorId, decimal totalAmount)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        VendorId = vendorId;
        TotalAmount = totalAmount;
    }
}

public class PurchaseOrderApprovedEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public Guid VendorId { get; }

    public PurchaseOrderApprovedEvent(Guid purchaseOrderId, string orderNumber, Guid vendorId)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        VendorId = vendorId;
    }
}

public class PurchaseOrderSentEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public Guid VendorId { get; }

    public PurchaseOrderSentEvent(Guid purchaseOrderId, string orderNumber, Guid vendorId)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        VendorId = vendorId;
    }
}

public class PurchaseOrderReceivedEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public bool IsFullyReceived { get; }

    public PurchaseOrderReceivedEvent(Guid purchaseOrderId, string orderNumber, bool isFullyReceived)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        IsFullyReceived = isFullyReceived;
    }
}

public class PurchaseOrderCompletedEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public decimal TotalAmount { get; }

    public PurchaseOrderCompletedEvent(Guid purchaseOrderId, string orderNumber, decimal totalAmount)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        TotalAmount = totalAmount;
    }
}

public class PurchaseOrderCancelledEvent : DomainEvent
{
    public Guid PurchaseOrderId { get; }
    public string OrderNumber { get; }
    public string Reason { get; }

    public PurchaseOrderCancelledEvent(Guid purchaseOrderId, string orderNumber, string reason)
    {
        PurchaseOrderId = purchaseOrderId;
        OrderNumber = orderNumber;
        Reason = reason;
    }
}