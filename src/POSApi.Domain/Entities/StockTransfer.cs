using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class StockTransfer : AggregateRoot
{
    public string TransferNumber { get; private set; } = string.Empty;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid FromLocationId { get; private set; }
    public Location FromLocation { get; private set; } = null!;
    public Guid ToLocationId { get; private set; }
    public Location ToLocation { get; private set; } = null!;
    public int RequestedQuantity { get; private set; }
    public int ShippedQuantity { get; private set; }
    public int ReceivedQuantity { get; private set; }
    public TransferStatus Status { get; private set; }
    public DateTime RequestedDate { get; private set; }
    public DateTime? ShippedDate { get; private set; }
    public DateTime? ReceivedDate { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public User RequestedBy { get; private set; } = null!;
    public Guid? ApprovedByUserId { get; private set; }
    public User? ApprovedBy { get; private set; }
    public Guid? ShippedByUserId { get; private set; }
    public User? ShippedBy { get; private set; }
    public Guid? ReceivedByUserId { get; private set; }
    public User? ReceivedBy { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string ReceiverNotes { get; private set; } = string.Empty;
    public decimal? ShippingCost { get; private set; }
    public string TrackingNumber { get; private set; } = string.Empty;

    private StockTransfer() { } // For EF Core

    public StockTransfer(string transferNumber, Guid productId, Guid fromLocationId, Guid toLocationId,
                        int requestedQuantity, Guid requestedByUserId, string notes = "")
    {
        if (fromLocationId == toLocationId)
        {
            throw new InvalidOperationException("Cannot transfer to the same location");
        }

        if (requestedQuantity <= 0)
        {
            throw new ArgumentException("Requested quantity must be greater than zero");
        }

        TransferNumber = transferNumber;
        ProductId = productId;
        FromLocationId = fromLocationId;
        ToLocationId = toLocationId;
        RequestedQuantity = requestedQuantity;
        RequestedByUserId = requestedByUserId;
        Notes = notes;
        Status = TransferStatus.Pending;
        RequestedDate = DateTime.UtcNow;

        AddDomainEvent(new StockTransferCreatedEvent(Id, transferNumber, productId, fromLocationId, toLocationId, requestedQuantity));
    }

    public void Approve(Guid approvedByUserId)
    {
        if (Status != TransferStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot approve transfer in {Status} status");
        }

        Status = TransferStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        SetUpdatedAt();

        AddDomainEvent(new StockTransferApprovedEvent(Id, TransferNumber, approvedByUserId));
    }

    public void Reject(Guid rejectedByUserId, string reason)
    {
        if (Status != TransferStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject transfer in {Status} status");
        }

        Status = TransferStatus.Rejected;
        ApprovedByUserId = rejectedByUserId;
        Notes = $"Rejected: {reason}. {Notes}";
        SetUpdatedAt();

        AddDomainEvent(new StockTransferRejectedEvent(Id, TransferNumber, rejectedByUserId, reason));
    }

    public void Ship(Guid shippedByUserId, int shippedQuantity, string trackingNumber = "", decimal? shippingCost = null)
    {
        if (Status != TransferStatus.Approved)
        {
            throw new InvalidOperationException($"Cannot ship transfer in {Status} status");
        }

        if (shippedQuantity <= 0 || shippedQuantity > RequestedQuantity)
        {
            throw new ArgumentException($"Shipped quantity must be between 1 and {RequestedQuantity}");
        }

        Status = TransferStatus.InTransit;
        ShippedByUserId = shippedByUserId;
        ShippedQuantity = shippedQuantity;
        ShippedDate = DateTime.UtcNow;
        TrackingNumber = trackingNumber;
        ShippingCost = shippingCost;
        SetUpdatedAt();

        AddDomainEvent(new StockTransferShippedEvent(Id, TransferNumber, shippedQuantity, shippedByUserId));
    }

    public void Receive(Guid receivedByUserId, int receivedQuantity, string receiverNotes = "")
    {
        if (Status != TransferStatus.InTransit)
        {
            throw new InvalidOperationException($"Cannot receive transfer in {Status} status");
        }

        if (receivedQuantity <= 0 || receivedQuantity > ShippedQuantity)
        {
            throw new ArgumentException($"Received quantity must be between 1 and {ShippedQuantity}");
        }

        Status = TransferStatus.Received;
        ReceivedByUserId = receivedByUserId;
        ReceivedQuantity = receivedQuantity;
        ReceivedDate = DateTime.UtcNow;
        ReceiverNotes = receiverNotes;
        SetUpdatedAt();

        AddDomainEvent(new StockTransferReceivedEvent(Id, TransferNumber, receivedQuantity, receivedByUserId, HasVariance));
    }

    public void Complete()
    {
        if (Status != TransferStatus.Received)
        {
            throw new InvalidOperationException($"Cannot complete transfer in {Status} status");
        }

        Status = TransferStatus.Completed;
        SetUpdatedAt();

        AddDomainEvent(new StockTransferCompletedEvent(Id, TransferNumber));
    }

    public void Cancel(Guid cancelledByUserId, string reason)
    {
        if (Status == TransferStatus.Completed || Status == TransferStatus.Received)
        {
            throw new InvalidOperationException($"Cannot cancel transfer in {Status} status");
        }

        Status = TransferStatus.Cancelled;
        Notes = $"Cancelled by user: {reason}. {Notes}";
        SetUpdatedAt();

        AddDomainEvent(new StockTransferCancelledEvent(Id, TransferNumber, cancelledByUserId, reason));
    }

    public int Variance => ReceivedQuantity - ShippedQuantity;
    public bool HasVariance => Variance != 0;
    public decimal VariancePercentage => ShippedQuantity > 0 ? (Math.Abs(Variance) / (decimal)ShippedQuantity) * 100 : 0;
}

public enum TransferStatus
{
    Pending,
    Approved,
    Rejected,
    InTransit,
    Received,
    Completed,
    Cancelled
}
