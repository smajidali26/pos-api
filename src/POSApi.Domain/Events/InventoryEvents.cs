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