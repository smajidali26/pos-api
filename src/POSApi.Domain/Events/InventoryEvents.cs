using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class InventoryMovementRecordedEvent : DomainEvent
{
    public Guid MovementId { get; }
    public Guid ProductId { get; }
    public MovementType Type { get; }
    public int Quantity { get; }
    public int NewQuantity { get; }

    public InventoryMovementRecordedEvent(Guid movementId, Guid productId, MovementType type, int quantity, int newQuantity)
    {
        MovementId = movementId;
        ProductId = productId;
        Type = type;
        Quantity = quantity;
        NewQuantity = newQuantity;
    }
}

public class LocationCreatedEvent : DomainEvent
{
    public Guid LocationId { get; }
    public string LocationName { get; }
    public string LocationCode { get; }
    public LocationType Type { get; }

    public LocationCreatedEvent(Guid locationId, string locationName, string locationCode, LocationType type)
    {
        LocationId = locationId;
        LocationName = locationName;
        LocationCode = locationCode;
        Type = type;
    }
}

public class StockCountCreatedEvent : DomainEvent
{
    public Guid StockCountId { get; }
    public string CountNumber { get; }
    public Guid LocationId { get; }

    public StockCountCreatedEvent(Guid stockCountId, string countNumber, Guid locationId)
    {
        StockCountId = stockCountId;
        CountNumber = countNumber;
        LocationId = locationId;
    }
}

public class StockCountCompletedEvent : DomainEvent
{
    public Guid StockCountId { get; }
    public string CountNumber { get; }
    public int ItemsCount { get; }

    public StockCountCompletedEvent(Guid stockCountId, string countNumber, int itemsCount)
    {
        StockCountId = stockCountId;
        CountNumber = countNumber;
        ItemsCount = itemsCount;
    }
}

public class LowStockDetectedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public Guid LocationId { get; }
    public string LocationName { get; }
    public int CurrentQuantity { get; }
    public int MinStockLevel { get; }

    public LowStockDetectedEvent(Guid productId, string productName, Guid locationId, string locationName, int currentQuantity, int minStockLevel)
    {
        ProductId = productId;
        ProductName = productName;
        LocationId = locationId;
        LocationName = locationName;
        CurrentQuantity = currentQuantity;
        MinStockLevel = minStockLevel;
    }
}

public class StockVarianceDetectedEvent : DomainEvent
{
    public Guid StockCountId { get; }
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CountedQuantity { get; }
    public int SystemQuantity { get; }
    public int Variance { get; }

    public StockVarianceDetectedEvent(Guid stockCountId, Guid productId, string productName, int countedQuantity, int systemQuantity, int variance)
    {
        StockCountId = stockCountId;
        ProductId = productId;
        ProductName = productName;
        CountedQuantity = countedQuantity;
        SystemQuantity = systemQuantity;
        Variance = variance;
    }
}

// Stock Transfer Events
public class StockTransferCreatedEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }
    public Guid ProductId { get; }
    public Guid FromLocationId { get; }
    public Guid ToLocationId { get; }
    public int RequestedQuantity { get; }

    public StockTransferCreatedEvent(Guid transferId, string transferNumber, Guid productId, Guid fromLocationId, Guid toLocationId, int requestedQuantity)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
        ProductId = productId;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
        RequestedQuantity = requestedQuantity;
    }
}

public class StockTransferApprovedEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }
    public Guid ApprovedByUserId { get; }

    public StockTransferApprovedEvent(Guid transferId, string transferNumber, Guid approvedByUserId)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
        ApprovedByUserId = approvedByUserId;
    }
}

public class StockTransferRejectedEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }
    public Guid RejectedByUserId { get; }
    public string Reason { get; }

    public StockTransferRejectedEvent(Guid transferId, string transferNumber, Guid rejectedByUserId, string reason)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
        RejectedByUserId = rejectedByUserId;
        Reason = reason;
    }
}

public class StockTransferShippedEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }
    public int ShippedQuantity { get; }
    public Guid ShippedByUserId { get; }

    public StockTransferShippedEvent(Guid transferId, string transferNumber, int shippedQuantity, Guid shippedByUserId)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
        ShippedQuantity = shippedQuantity;
        ShippedByUserId = shippedByUserId;
    }
}

public class StockTransferReceivedEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }
    public int ReceivedQuantity { get; }
    public Guid ReceivedByUserId { get; }
    public bool HasVariance { get; }

    public StockTransferReceivedEvent(Guid transferId, string transferNumber, int receivedQuantity, Guid receivedByUserId, bool hasVariance)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
        ReceivedQuantity = receivedQuantity;
        ReceivedByUserId = receivedByUserId;
        HasVariance = hasVariance;
    }
}

public class StockTransferCompletedEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }

    public StockTransferCompletedEvent(Guid transferId, string transferNumber)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
    }
}

public class StockTransferCancelledEvent : DomainEvent
{
    public Guid TransferId { get; }
    public string TransferNumber { get; }
    public Guid CancelledByUserId { get; }
    public string Reason { get; }

    public StockTransferCancelledEvent(Guid transferId, string transferNumber, Guid cancelledByUserId, string reason)
    {
        TransferId = transferId;
        TransferNumber = transferNumber;
        CancelledByUserId = cancelledByUserId;
        Reason = reason;
    }
}

// Batch/Lot Events
public class BatchCreatedEvent : DomainEvent
{
    public Guid BatchId { get; }
    public string BatchNumber { get; }
    public Guid ProductId { get; }
    public int InitialQuantity { get; }
    public DateTime? ExpiryDate { get; }

    public BatchCreatedEvent(Guid batchId, string batchNumber, Guid productId, int initialQuantity, DateTime? expiryDate)
    {
        BatchId = batchId;
        BatchNumber = batchNumber;
        ProductId = productId;
        InitialQuantity = initialQuantity;
        ExpiryDate = expiryDate;
    }
}

public class BatchDepletedEvent : DomainEvent
{
    public Guid BatchId { get; }
    public string BatchNumber { get; }
    public Guid ProductId { get; }

    public BatchDepletedEvent(Guid batchId, string batchNumber, Guid productId)
    {
        BatchId = batchId;
        BatchNumber = batchNumber;
        ProductId = productId;
    }
}

public class BatchExpiredEvent : DomainEvent
{
    public Guid BatchId { get; }
    public string BatchNumber { get; }
    public Guid ProductId { get; }
    public int RemainingQuantity { get; }

    public BatchExpiredEvent(Guid batchId, string batchNumber, Guid productId, int remainingQuantity)
    {
        BatchId = batchId;
        BatchNumber = batchNumber;
        ProductId = productId;
        RemainingQuantity = remainingQuantity;
    }
}

public class BatchRecalledEvent : DomainEvent
{
    public Guid BatchId { get; }
    public string BatchNumber { get; }
    public Guid ProductId { get; }
    public string Reason { get; }
    public int AffectedQuantity { get; }
    public Guid RecalledByUserId { get; }

    public BatchRecalledEvent(Guid batchId, string batchNumber, Guid productId, string reason, int affectedQuantity, Guid recalledByUserId)
    {
        BatchId = batchId;
        BatchNumber = batchNumber;
        ProductId = productId;
        Reason = reason;
        AffectedQuantity = affectedQuantity;
        RecalledByUserId = recalledByUserId;
    }
}

// Serial Number Events
public class SerialNumberCreatedEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }

    public SerialNumberCreatedEvent(Guid serialNumberId, string number, Guid productId)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
    }
}

public class SerialNumberSoldEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }
    public Guid CustomerId { get; }
    public Guid OrderId { get; }

    public SerialNumberSoldEvent(Guid serialNumberId, string number, Guid productId, Guid customerId, Guid orderId)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
        CustomerId = customerId;
        OrderId = orderId;
    }
}

public class SerialNumberReturnedEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }
    public Guid? CustomerId { get; }
    public string Reason { get; }

    public SerialNumberReturnedEvent(Guid serialNumberId, string number, Guid productId, Guid? customerId, string reason)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
        CustomerId = customerId;
        Reason = reason;
    }
}

public class SerialNumberDefectiveEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }
    public string Reason { get; }

    public SerialNumberDefectiveEvent(Guid serialNumberId, string number, Guid productId, string reason)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
        Reason = reason;
    }
}

public class SerialNumberRepairedEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }

    public SerialNumberRepairedEvent(Guid serialNumberId, string number, Guid productId)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
    }
}

public class SerialNumberTransferredEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }
    public Guid? FromLocationId { get; }
    public Guid ToLocationId { get; }

    public SerialNumberTransferredEvent(Guid serialNumberId, string number, Guid productId, Guid? fromLocationId, Guid toLocationId)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
    }
}

public class SerialNumberDisposedEvent : DomainEvent
{
    public Guid SerialNumberId { get; }
    public string Number { get; }
    public Guid ProductId { get; }
    public string Reason { get; }

    public SerialNumberDisposedEvent(Guid serialNumberId, string number, Guid productId, string reason)
    {
        SerialNumberId = serialNumberId;
        Number = number;
        ProductId = productId;
        Reason = reason;
    }
}

// Stock Alert Events
public class StockAlertTriggeredEvent : DomainEvent
{
    public Guid AlertId { get; }
    public Guid ProductId { get; }
    public StockAlertType AlertType { get; }
    public int CurrentQuantity { get; }
    public int ThresholdQuantity { get; }
    public StockAlertSeverity Severity { get; }
    public Guid? LocationId { get; }

    public StockAlertTriggeredEvent(Guid alertId, Guid productId, StockAlertType alertType, int currentQuantity, int thresholdQuantity, StockAlertSeverity severity, Guid? locationId)
    {
        AlertId = alertId;
        ProductId = productId;
        AlertType = alertType;
        CurrentQuantity = currentQuantity;
        ThresholdQuantity = thresholdQuantity;
        Severity = severity;
        LocationId = locationId;
    }
}

public class StockAlertAcknowledgedEvent : DomainEvent
{
    public Guid AlertId { get; }
    public Guid ProductId { get; }
    public Guid AcknowledgedByUserId { get; }

    public StockAlertAcknowledgedEvent(Guid alertId, Guid productId, Guid acknowledgedByUserId)
    {
        AlertId = alertId;
        ProductId = productId;
        AcknowledgedByUserId = acknowledgedByUserId;
    }
}

public class StockAlertResolvedEvent : DomainEvent
{
    public Guid AlertId { get; }
    public Guid ProductId { get; }
    public Guid ResolvedByUserId { get; }

    public StockAlertResolvedEvent(Guid alertId, Guid productId, Guid resolvedByUserId)
    {
        AlertId = alertId;
        ProductId = productId;
        ResolvedByUserId = resolvedByUserId;
    }
}

public class StockAlertDismissedEvent : DomainEvent
{
    public Guid AlertId { get; }
    public Guid ProductId { get; }
    public Guid DismissedByUserId { get; }
    public string Reason { get; }

    public StockAlertDismissedEvent(Guid alertId, Guid productId, Guid dismissedByUserId, string reason)
    {
        AlertId = alertId;
        ProductId = productId;
        DismissedByUserId = dismissedByUserId;
        Reason = reason;
    }
}