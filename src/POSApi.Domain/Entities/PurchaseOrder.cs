using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class PurchaseOrder : AggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid VendorId { get; private set; }
    public Vendor Vendor { get; private set; } = null!;
    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? ActualDeliveryDate { get; private set; }
    public PurchaseOrderStatus Status { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal ShippingCost { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; private set; } = new List<PurchaseOrderItem>();

    private PurchaseOrder() { } // For EF Core

    public PurchaseOrder(string orderNumber, Guid vendorId, Guid createdByUserId, 
                        DateTime? expectedDeliveryDate = null)
    {
        OrderNumber = orderNumber;
        VendorId = vendorId;
        CreatedByUserId = createdByUserId;
        OrderDate = DateTime.UtcNow;
        ExpectedDeliveryDate = expectedDeliveryDate ?? DateTime.UtcNow.AddDays(7);
        Status = PurchaseOrderStatus.Draft;

        AddDomainEvent(new PurchaseOrderCreatedEvent(Id, orderNumber, vendorId));
    }

    public void AddItem(Guid productId, int quantity, decimal unitCost)
    {
        var existingItem = Items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = new PurchaseOrderItem(Id, productId, quantity, unitCost);
            Items.Add(item);
        }

        RecalculateTotal();
        SetUpdatedAt();
    }

    public void RemoveItem(Guid productId)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            Items.Remove(item);
            RecalculateTotal();
            SetUpdatedAt();
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                RemoveItem(productId);
            }
            else
            {
                item.UpdateQuantity(quantity);
                RecalculateTotal();
                SetUpdatedAt();
            }
        }
    }

    public void Submit()
    {
        if (Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot submit purchase order in {Status} status");
        }

        if (!Items.Any())
        {
            throw new InvalidOperationException("Cannot submit purchase order without items");
        }

        Status = PurchaseOrderStatus.Submitted;
        SetUpdatedAt();

        AddDomainEvent(new PurchaseOrderSubmittedEvent(Id, OrderNumber, VendorId, TotalAmount));
    }

    public void Approve()
    {
        if (Status != PurchaseOrderStatus.Submitted)
        {
            throw new InvalidOperationException($"Cannot approve purchase order in {Status} status");
        }

        Status = PurchaseOrderStatus.Approved;
        SetUpdatedAt();

        AddDomainEvent(new PurchaseOrderApprovedEvent(Id, OrderNumber, VendorId));
    }

    public void Send()
    {
        if (Status != PurchaseOrderStatus.Approved)
        {
            throw new InvalidOperationException($"Cannot send purchase order in {Status} status");
        }

        Status = PurchaseOrderStatus.Sent;
        SetUpdatedAt();

        AddDomainEvent(new PurchaseOrderSentEvent(Id, OrderNumber, VendorId));
    }

    public void ReceivePartial(Dictionary<Guid, int> receivedQuantities)
    {
        if (Status != PurchaseOrderStatus.Sent && Status != PurchaseOrderStatus.PartiallyReceived)
        {
            throw new InvalidOperationException($"Cannot receive items for purchase order in {Status} status");
        }

        foreach (var (productId, receivedQty) in receivedQuantities)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                item.ReceiveQuantity(receivedQty);
            }
        }

        var allReceived = Items.All(i => i.ReceivedQuantity >= i.Quantity);
        Status = allReceived ? PurchaseOrderStatus.Received : PurchaseOrderStatus.PartiallyReceived;
        
        if (Status == PurchaseOrderStatus.Received)
        {
            ActualDeliveryDate = DateTime.UtcNow;
        }

        SetUpdatedAt();

        AddDomainEvent(new PurchaseOrderReceivedEvent(Id, OrderNumber, Status == PurchaseOrderStatus.Received));
    }

    public void Complete()
    {
        if (Status != PurchaseOrderStatus.Received)
        {
            throw new InvalidOperationException($"Cannot complete purchase order in {Status} status");
        }

        Status = PurchaseOrderStatus.Completed;
        SetUpdatedAt();

        AddDomainEvent(new PurchaseOrderCompletedEvent(Id, OrderNumber, TotalAmount));
    }

    public void Cancel(string reason)
    {
        if (Status == PurchaseOrderStatus.Completed || Status == PurchaseOrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot cancel purchase order in {Status} status");
        }

        Status = PurchaseOrderStatus.Cancelled;
        Notes = $"Cancelled: {reason}. {Notes}";
        SetUpdatedAt();

        AddDomainEvent(new PurchaseOrderCancelledEvent(Id, OrderNumber, reason));
    }

    public void UpdateExpectedDeliveryDate(DateTime expectedDeliveryDate)
    {
        ExpectedDeliveryDate = expectedDeliveryDate;
        SetUpdatedAt();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
        RecalculateTotal();
        SetUpdatedAt();
    }

    public void UpdateShippingCost(decimal shippingCost)
    {
        ShippingCost = shippingCost;
        RecalculateTotal();
        SetUpdatedAt();
    }

    private void RecalculateTotal()
    {
        SubTotal = Items.Sum(i => i.TotalCost);
        TaxAmount = SubTotal * 0.08m; // 8% tax rate - should be configurable
        TotalAmount = SubTotal + TaxAmount + ShippingCost - DiscountAmount;
    }

    public bool IsOverdue => ExpectedDeliveryDate.HasValue && 
                            ExpectedDeliveryDate < DateTime.UtcNow && 
                            Status != PurchaseOrderStatus.Received && 
                            Status != PurchaseOrderStatus.Completed &&
                            Status != PurchaseOrderStatus.Cancelled;
}

public enum PurchaseOrderStatus
{
    Draft,
    Submitted,
    Approved,
    Sent,
    PartiallyReceived,
    Received,
    Completed,
    Cancelled
}