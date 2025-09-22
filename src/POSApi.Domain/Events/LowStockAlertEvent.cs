using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class LowStockAlertEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public int CurrentStock { get; }
    public int MinStockLevel { get; }

    public LowStockAlertEvent(Guid productId, string productName, int currentStock, int minStockLevel)
    {
        ProductId = productId;
        ProductName = productName;
        CurrentStock = currentStock;
        MinStockLevel = minStockLevel;
    }
}