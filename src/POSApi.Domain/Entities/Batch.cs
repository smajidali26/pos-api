using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Batch : AggregateRoot
{
    public string BatchNumber { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int InitialQuantity { get; private set; }
    public int CurrentQuantity { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }
    public DateTime? ManufactureDate { get; private set; }
    public Guid? VendorId { get; private set; }
    public Vendor? Vendor { get; private set; }
    public decimal UnitCost { get; private set; }
    public Guid? PurchaseOrderId { get; private set; }
    public PurchaseOrder? PurchaseOrder { get; private set; }
    public BatchStatus Status { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;
    public bool IsRecalled { get; private set; }
    public string RecallReason { get; private set; } = string.Empty;
    public DateTime? RecallDate { get; private set; }

    public ICollection<BatchMovement> BatchMovements { get; private set; } = new List<BatchMovement>();
    public ICollection<SerialNumber> SerialNumbers { get; private set; } = new List<SerialNumber>();

    private Batch() { } // For EF Core

    public Batch(string batchNumber, Guid productId, int initialQuantity, decimal unitCost,
                Guid createdByUserId, DateTime? expiryDate = null, DateTime? manufactureDate = null,
                Guid? vendorId = null, Guid? purchaseOrderId = null, string notes = "")
    {
        if (initialQuantity <= 0)
        {
            throw new ArgumentException("Initial quantity must be greater than zero");
        }

        if (unitCost < 0)
        {
            throw new ArgumentException("Unit cost cannot be negative");
        }

        BatchNumber = batchNumber;
        ProductId = productId;
        InitialQuantity = initialQuantity;
        CurrentQuantity = initialQuantity;
        UnitCost = unitCost;
        ReceivedDate = DateTime.UtcNow;
        ExpiryDate = expiryDate;
        ManufactureDate = manufactureDate;
        VendorId = vendorId;
        PurchaseOrderId = purchaseOrderId;
        CreatedByUserId = createdByUserId;
        Notes = notes;
        Status = BatchStatus.Active;
        IsRecalled = false;

        AddDomainEvent(new BatchCreatedEvent(Id, batchNumber, productId, initialQuantity, expiryDate));
    }

    public void ReduceQuantity(int quantity, string reason, Guid userId, Guid? referenceId = null)
    {
        if (Status != BatchStatus.Active)
        {
            throw new InvalidOperationException($"Cannot reduce quantity from {Status} batch");
        }

        if (IsRecalled)
        {
            throw new InvalidOperationException("Cannot reduce quantity from recalled batch");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (quantity > CurrentQuantity)
        {
            throw new InvalidOperationException($"Insufficient quantity in batch. Available: {CurrentQuantity}, Requested: {quantity}");
        }

        var previousQuantity = CurrentQuantity;
        CurrentQuantity -= quantity;

        var movement = new BatchMovement(Id, BatchMovementType.Reduction, quantity, previousQuantity, userId, reason, referenceId);
        BatchMovements.Add(movement);

        if (CurrentQuantity == 0)
        {
            Status = BatchStatus.Depleted;
            AddDomainEvent(new BatchDepletedEvent(Id, BatchNumber, ProductId));
        }

        SetUpdatedAt();
    }

    public void IncreaseQuantity(int quantity, string reason, Guid userId, Guid? referenceId = null)
    {
        if (Status == BatchStatus.Expired || Status == BatchStatus.Recalled)
        {
            throw new InvalidOperationException($"Cannot increase quantity for {Status} batch");
        }

        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        var previousQuantity = CurrentQuantity;
        CurrentQuantity += quantity;

        if (Status == BatchStatus.Depleted)
        {
            Status = BatchStatus.Active;
        }

        var movement = new BatchMovement(Id, BatchMovementType.Addition, quantity, previousQuantity, userId, reason, referenceId);
        BatchMovements.Add(movement);

        SetUpdatedAt();
    }

    public void MarkAsExpired()
    {
        if (Status == BatchStatus.Recalled)
        {
            throw new InvalidOperationException("Cannot expire a recalled batch");
        }

        Status = BatchStatus.Expired;
        SetUpdatedAt();

        AddDomainEvent(new BatchExpiredEvent(Id, BatchNumber, ProductId, CurrentQuantity));
    }

    public void Recall(string reason, Guid recalledByUserId)
    {
        if (IsRecalled)
        {
            throw new InvalidOperationException("Batch is already recalled");
        }

        IsRecalled = true;
        RecallReason = reason;
        RecallDate = DateTime.UtcNow;
        Status = BatchStatus.Recalled;
        SetUpdatedAt();

        AddDomainEvent(new BatchRecalledEvent(Id, BatchNumber, ProductId, reason, CurrentQuantity, recalledByUserId));
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;
    public bool IsExpiringSoon => ExpiryDate.HasValue && ExpiryDate.Value <= DateTime.UtcNow.AddDays(7) && ExpiryDate.Value > DateTime.UtcNow;
    public int DaysUntilExpiry => ExpiryDate.HasValue ? (ExpiryDate.Value - DateTime.UtcNow).Days : int.MaxValue;
    public decimal TotalValue => CurrentQuantity * UnitCost;
}

public class BatchMovement : BaseEntity
{
    public Guid BatchId { get; private set; }
    public Batch Batch { get; private set; } = null!;
    public BatchMovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public int PreviousQuantity { get; private set; }
    public int NewQuantity { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public Guid MovedByUserId { get; private set; }
    public User MovedBy { get; private set; } = null!;
    public DateTime MovementDate { get; private set; }
    public Guid? ReferenceId { get; private set; }

    private BatchMovement() { } // For EF Core

    public BatchMovement(Guid batchId, BatchMovementType type, int quantity, int previousQuantity,
                        Guid movedByUserId, string reason, Guid? referenceId = null)
    {
        BatchId = batchId;
        Type = type;
        Quantity = Math.Abs(quantity);
        PreviousQuantity = previousQuantity;
        MovedByUserId = movedByUserId;
        Reason = reason;
        ReferenceId = referenceId;
        MovementDate = DateTime.UtcNow;

        NewQuantity = type switch
        {
            BatchMovementType.Addition => previousQuantity + Quantity,
            BatchMovementType.Reduction => previousQuantity - Quantity,
            _ => previousQuantity
        };
    }
}

public enum BatchStatus
{
    Active,
    Depleted,
    Expired,
    Recalled
}

public enum BatchMovementType
{
    Addition,
    Reduction
}
