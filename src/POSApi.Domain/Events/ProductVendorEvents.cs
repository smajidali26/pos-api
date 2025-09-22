using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class ProductCostChangedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public decimal OldCost { get; }
    public decimal NewCost { get; }

    public ProductCostChangedEvent(Guid productId, decimal oldCost, decimal newCost)
    {
        ProductId = productId;
        OldCost = oldCost;
        NewCost = newCost;
    }
}

public class ReorderPointReachedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentStock { get; }
    public int ReorderLevel { get; }
    public int ReorderQuantity { get; }
    public Guid? PrimaryVendorId { get; }

    public ReorderPointReachedEvent(Guid productId, string productName, int currentStock, 
                                   int reorderLevel, int reorderQuantity, Guid? primaryVendorId)
    {
        ProductId = productId;
        ProductName = productName;
        CurrentStock = currentStock;
        ReorderLevel = reorderLevel;
        ReorderQuantity = reorderQuantity;
        PrimaryVendorId = primaryVendorId;
    }
}

public class ProductVendorChangedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public Guid? OldVendorId { get; }
    public Guid? NewVendorId { get; }

    public ProductVendorChangedEvent(Guid productId, Guid? oldVendorId, Guid? newVendorId)
    {
        ProductId = productId;
        OldVendorId = oldVendorId;
        NewVendorId = newVendorId;
    }
}

public class ProductPurchaseRecordedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public decimal PurchaseCost { get; }
    public DateTime PurchaseDate { get; }

    public ProductPurchaseRecordedEvent(Guid productId, decimal purchaseCost, DateTime purchaseDate)
    {
        ProductId = productId;
        PurchaseCost = purchaseCost;
        PurchaseDate = purchaseDate;
    }
}