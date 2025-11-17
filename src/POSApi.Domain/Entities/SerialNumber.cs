using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class SerialNumber : AggregateRoot
{
    public string Number { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? BatchId { get; private set; }
    public Batch? Batch { get; private set; }
    public SerialNumberStatus Status { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public Guid? OrderId { get; private set; }
    public Order? Order { get; private set; }
    public DateTime? SoldDate { get; private set; }
    public DateTime? ReturnedDate { get; private set; }
    public Guid? LocationId { get; private set; }
    public Location? Location { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public DateTime? WarrantyStartDate { get; private set; }
    public DateTime? WarrantyEndDate { get; private set; }
    public int WarrantyMonths { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;

    public ICollection<SerialNumberHistory> History { get; private set; } = new List<SerialNumberHistory>();

    private SerialNumber() { } // For EF Core

    public SerialNumber(string number, Guid productId, Guid createdByUserId, Guid? batchId = null,
                       Guid? locationId = null, int warrantyMonths = 0, string notes = "")
    {
        if (string.IsNullOrWhiteSpace(number))
        {
            throw new ArgumentException("Serial number cannot be empty");
        }

        Number = number;
        ProductId = productId;
        BatchId = batchId;
        LocationId = locationId;
        CreatedByUserId = createdByUserId;
        WarrantyMonths = warrantyMonths;
        Notes = notes;
        Status = SerialNumberStatus.Available;

        AddHistoryEntry(SerialNumberAction.Created, createdByUserId, "Serial number created");
        AddDomainEvent(new SerialNumberCreatedEvent(Id, number, productId));
    }

    public void Sell(Guid customerId, Guid orderId, Guid soldByUserId)
    {
        if (Status != SerialNumberStatus.Available)
        {
            throw new InvalidOperationException($"Cannot sell serial number in {Status} status");
        }

        var previousStatus = Status;
        Status = SerialNumberStatus.Sold;
        CustomerId = customerId;
        OrderId = orderId;
        SoldDate = DateTime.UtcNow;

        if (WarrantyMonths > 0)
        {
            WarrantyStartDate = DateTime.UtcNow;
            WarrantyEndDate = DateTime.UtcNow.AddMonths(WarrantyMonths);
        }

        AddHistoryEntry(SerialNumberAction.Sold, soldByUserId, $"Sold to customer via order {orderId}");
        SetUpdatedAt();

        AddDomainEvent(new SerialNumberSoldEvent(Id, Number, ProductId, customerId, orderId));
    }

    public void Return(Guid returnedByUserId, string reason)
    {
        if (Status != SerialNumberStatus.Sold)
        {
            throw new InvalidOperationException($"Cannot return serial number in {Status} status");
        }

        Status = SerialNumberStatus.Returned;
        ReturnedDate = DateTime.UtcNow;

        AddHistoryEntry(SerialNumberAction.Returned, returnedByUserId, $"Returned: {reason}");
        SetUpdatedAt();

        AddDomainEvent(new SerialNumberReturnedEvent(Id, Number, ProductId, CustomerId, reason));
    }

    public void MarkAsDefective(Guid markedByUserId, string reason)
    {
        if (Status == SerialNumberStatus.Disposed)
        {
            throw new InvalidOperationException("Cannot mark disposed serial number as defective");
        }

        var previousStatus = Status;
        Status = SerialNumberStatus.Defective;

        AddHistoryEntry(SerialNumberAction.MarkedDefective, markedByUserId, $"Marked as defective: {reason}");
        SetUpdatedAt();

        AddDomainEvent(new SerialNumberDefectiveEvent(Id, Number, ProductId, reason));
    }

    public void Repair(Guid repairedByUserId, string notes)
    {
        if (Status != SerialNumberStatus.Defective && Status != SerialNumberStatus.Returned)
        {
            throw new InvalidOperationException($"Cannot repair serial number in {Status} status");
        }

        Status = SerialNumberStatus.Available;

        AddHistoryEntry(SerialNumberAction.Repaired, repairedByUserId, $"Repaired: {notes}");
        SetUpdatedAt();

        AddDomainEvent(new SerialNumberRepairedEvent(Id, Number, ProductId));
    }

    public void Transfer(Guid toLocationId, Guid transferredByUserId, string reason = "")
    {
        if (Status == SerialNumberStatus.Sold || Status == SerialNumberStatus.Disposed)
        {
            throw new InvalidOperationException($"Cannot transfer serial number in {Status} status");
        }

        var previousLocationId = LocationId;
        LocationId = toLocationId;

        AddHistoryEntry(SerialNumberAction.Transferred, transferredByUserId,
            $"Transferred from location {previousLocationId} to {toLocationId}. {reason}");
        SetUpdatedAt();

        AddDomainEvent(new SerialNumberTransferredEvent(Id, Number, ProductId, previousLocationId, toLocationId));
    }

    public void Dispose(Guid disposedByUserId, string reason)
    {
        if (Status == SerialNumberStatus.Disposed)
        {
            throw new InvalidOperationException("Serial number is already disposed");
        }

        Status = SerialNumberStatus.Disposed;

        AddHistoryEntry(SerialNumberAction.Disposed, disposedByUserId, $"Disposed: {reason}");
        SetUpdatedAt();

        AddDomainEvent(new SerialNumberDisposedEvent(Id, Number, ProductId, reason));
    }

    public void UpdateWarranty(int warrantyMonths)
    {
        WarrantyMonths = warrantyMonths;

        if (WarrantyStartDate.HasValue)
        {
            WarrantyEndDate = WarrantyStartDate.Value.AddMonths(warrantyMonths);
        }

        SetUpdatedAt();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    private void AddHistoryEntry(SerialNumberAction action, Guid userId, string notes)
    {
        var historyEntry = new SerialNumberHistory(Id, action, userId, notes);
        History.Add(historyEntry);
    }

    public bool IsUnderWarranty => WarrantyEndDate.HasValue && WarrantyEndDate.Value > DateTime.UtcNow;
    public int DaysRemainingInWarranty => WarrantyEndDate.HasValue
        ? Math.Max(0, (WarrantyEndDate.Value - DateTime.UtcNow).Days)
        : 0;
}

public class SerialNumberHistory : BaseEntity
{
    public Guid SerialNumberId { get; private set; }
    public SerialNumber SerialNumber { get; private set; } = null!;
    public SerialNumberAction Action { get; private set; }
    public Guid ActionByUserId { get; private set; }
    public User ActionBy { get; private set; } = null!;
    public DateTime ActionDate { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private SerialNumberHistory() { } // For EF Core

    public SerialNumberHistory(Guid serialNumberId, SerialNumberAction action, Guid actionByUserId, string notes)
    {
        SerialNumberId = serialNumberId;
        Action = action;
        ActionByUserId = actionByUserId;
        ActionDate = DateTime.UtcNow;
        Notes = notes;
    }
}

public enum SerialNumberStatus
{
    Available,
    Sold,
    Returned,
    Defective,
    Disposed
}

public enum SerialNumberAction
{
    Created,
    Sold,
    Returned,
    MarkedDefective,
    Repaired,
    Transferred,
    Disposed
}
