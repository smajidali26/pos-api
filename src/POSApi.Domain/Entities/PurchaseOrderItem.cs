using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public int ReceivedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost => Quantity * UnitCost;
    public bool IsFullyReceived => ReceivedQuantity >= Quantity;
    public int PendingQuantity => Math.Max(0, Quantity - ReceivedQuantity);

    private PurchaseOrderItem() { } // For EF Core

    public PurchaseOrderItem(Guid purchaseOrderId, Guid productId, int quantity, decimal unitCost)
    {
        PurchaseOrderId = purchaseOrderId;
        ProductId = productId;
        Quantity = quantity;
        UnitCost = unitCost;
        ReceivedQuantity = 0;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity < ReceivedQuantity)
        {
            throw new InvalidOperationException($"Cannot set quantity ({quantity}) below received quantity ({ReceivedQuantity})");
        }

        Quantity = quantity;
        SetUpdatedAt();
    }

    public void UpdateUnitCost(decimal unitCost)
    {
        UnitCost = unitCost;
        SetUpdatedAt();
    }

    public void ReceiveQuantity(int receivedQuantity)
    {
        if (receivedQuantity < 0)
        {
            throw new ArgumentException("Received quantity cannot be negative");
        }

        var newTotalReceived = ReceivedQuantity + receivedQuantity;
        if (newTotalReceived > Quantity)
        {
            throw new InvalidOperationException($"Cannot receive more than ordered quantity. Ordered: {Quantity}, Already received: {ReceivedQuantity}, Trying to receive: {receivedQuantity}");
        }

        ReceivedQuantity = newTotalReceived;
        SetUpdatedAt();
    }

    public void AdjustReceivedQuantity(int newReceivedQuantity)
    {
        if (newReceivedQuantity < 0 || newReceivedQuantity > Quantity)
        {
            throw new ArgumentException($"Received quantity must be between 0 and {Quantity}");
        }

        ReceivedQuantity = newReceivedQuantity;
        SetUpdatedAt();
    }
}