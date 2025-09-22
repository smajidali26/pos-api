using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Return : AggregateRoot
{
    public string ReturnNumber { get; private set; } = string.Empty;
    public Guid OriginalOrderId { get; private set; }
    public Order OriginalOrder { get; private set; } = null!;
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public DateTime ReturnDate { get; private set; }
    public ReturnReason Reason { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public decimal RefundAmount { get; private set; }
    public ReturnStatus Status { get; private set; }
    public RefundMethod RefundMethod { get; private set; }
    public Guid ProcessedByUserId { get; private set; }
    public User ProcessedBy { get; private set; } = null!;
    public ICollection<ReturnItem> ReturnItems { get; private set; } = new List<ReturnItem>();

    private Return() { } // For EF Core

    public Return(string returnNumber, Guid originalOrderId, Guid processedByUserId, ReturnReason reason, string notes = "")
    {
        ReturnNumber = returnNumber;
        OriginalOrderId = originalOrderId;
        ProcessedByUserId = processedByUserId;
        Reason = reason;
        Notes = notes;
        ReturnDate = DateTime.UtcNow;
        Status = ReturnStatus.Draft;

        AddDomainEvent(new ReturnCreatedEvent(Id, returnNumber, originalOrderId));
    }

    public void AddReturnItem(Guid productId, int quantity, decimal unitPrice, string reason = "")
    {
        var existingItem = ReturnItems.FirstOrDefault(x => x.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var returnItem = new ReturnItem(Id, productId, quantity, unitPrice, reason);
            ReturnItems.Add(returnItem);
        }

        RecalculateRefundAmount();
        SetUpdatedAt();
    }

    public void RemoveReturnItem(Guid productId)
    {
        var item = ReturnItems.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            ReturnItems.Remove(item);
            RecalculateRefundAmount();
            SetUpdatedAt();
        }
    }

    public void Process(RefundMethod refundMethod)
    {
        if (Status != ReturnStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot process return in {Status} status");
        }

        if (!ReturnItems.Any())
        {
            throw new InvalidOperationException("Cannot process return without items");
        }

        Status = ReturnStatus.Processed;
        RefundMethod = refundMethod;
        SetUpdatedAt();

        AddDomainEvent(new ReturnProcessedEvent(Id, ReturnNumber, RefundAmount, refundMethod));
    }

    public void Complete()
    {
        if (Status != ReturnStatus.Processed)
        {
            throw new InvalidOperationException($"Cannot complete return in {Status} status");
        }

        Status = ReturnStatus.Completed;
        SetUpdatedAt();

        AddDomainEvent(new ReturnCompletedEvent(Id, ReturnNumber, RefundAmount));
    }

    public void Cancel(string reason)
    {
        if (Status == ReturnStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel completed return");
        }

        Status = ReturnStatus.Cancelled;
        Notes = $"Cancelled: {reason}. {Notes}";
        SetUpdatedAt();

        AddDomainEvent(new ReturnCancelledEvent(Id, ReturnNumber, reason));
    }

    private void RecalculateRefundAmount()
    {
        RefundAmount = ReturnItems.Sum(x => x.TotalRefund);
    }
}

public class ReturnItem : BaseEntity
{
    public Guid ReturnId { get; private set; }
    public Return Return { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public decimal TotalRefund => Quantity * UnitPrice;

    private ReturnItem() { } // For EF Core

    public ReturnItem(Guid returnId, Guid productId, int quantity, decimal unitPrice, string reason = "")
    {
        ReturnId = returnId;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Reason = reason;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0");
        }

        Quantity = quantity;
        SetUpdatedAt();
    }
}

public enum ReturnStatus
{
    Draft,
    Processed,
    Completed,
    Cancelled
}

public enum ReturnReason
{
    DefectiveProduct,
    WrongItem,
    CustomerChanged,
    DamagedInTransit,
    ExpiredProduct,
    NotAsDescribed,
    Other
}

public enum RefundMethod
{
    Cash,
    CreditCard,
    StoreCredit,
    Exchange,
    BankTransfer
}