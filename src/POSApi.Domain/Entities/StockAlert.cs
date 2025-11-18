using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class StockAlert : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? LocationId { get; private set; }
    public Location? Location { get; private set; }
    public StockAlertType AlertType { get; private set; }
    public int CurrentQuantity { get; private set; }
    public int ThresholdQuantity { get; private set; }
    public StockAlertSeverity Severity { get; private set; }
    public StockAlertStatus Status { get; private set; }
    public DateTime TriggeredDate { get; private set; }
    public DateTime? AcknowledgedDate { get; private set; }
    public Guid? AcknowledgedByUserId { get; private set; }
    public User? AcknowledgedBy { get; private set; }
    public DateTime? ResolvedDate { get; private set; }
    public Guid? ResolvedByUserId { get; private set; }
    public User? ResolvedBy { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string ResolutionNotes { get; private set; } = string.Empty;

    private StockAlert() { } // For EF Core

    public StockAlert(Guid productId, StockAlertType alertType, int currentQuantity, int thresholdQuantity,
                     Guid? locationId = null)
    {
        ProductId = productId;
        AlertType = alertType;
        CurrentQuantity = currentQuantity;
        ThresholdQuantity = thresholdQuantity;
        LocationId = locationId;
        Status = StockAlertStatus.Active;
        TriggeredDate = DateTime.UtcNow;
        Severity = DetermineSeverity(alertType, currentQuantity, thresholdQuantity);

        AddDomainEvent(new StockAlertTriggeredEvent(Id, productId, alertType, currentQuantity, thresholdQuantity, Severity, locationId));
    }

    public void Acknowledge(Guid acknowledgedByUserId, string notes = "")
    {
        if (Status != StockAlertStatus.Active)
        {
            throw new InvalidOperationException($"Cannot acknowledge alert in {Status} status");
        }

        Status = StockAlertStatus.Acknowledged;
        AcknowledgedDate = DateTime.UtcNow;
        AcknowledgedByUserId = acknowledgedByUserId;
        Notes = notes;
        SetUpdatedAt();

        AddDomainEvent(new StockAlertAcknowledgedEvent(Id, ProductId, acknowledgedByUserId));
    }

    public void Resolve(Guid resolvedByUserId, string resolutionNotes)
    {
        if (Status == StockAlertStatus.Resolved || Status == StockAlertStatus.Dismissed)
        {
            throw new InvalidOperationException($"Cannot resolve alert in {Status} status");
        }

        Status = StockAlertStatus.Resolved;
        ResolvedDate = DateTime.UtcNow;
        ResolvedByUserId = resolvedByUserId;
        ResolutionNotes = resolutionNotes;
        SetUpdatedAt();

        AddDomainEvent(new StockAlertResolvedEvent(Id, ProductId, resolvedByUserId));
    }

    public void Dismiss(Guid dismissedByUserId, string reason)
    {
        if (Status == StockAlertStatus.Resolved)
        {
            throw new InvalidOperationException("Cannot dismiss a resolved alert");
        }

        Status = StockAlertStatus.Dismissed;
        ResolutionNotes = $"Dismissed: {reason}";
        SetUpdatedAt();

        AddDomainEvent(new StockAlertDismissedEvent(Id, ProductId, dismissedByUserId, reason));
    }

    public void UpdateQuantity(int newQuantity)
    {
        CurrentQuantity = newQuantity;
        Severity = DetermineSeverity(AlertType, newQuantity, ThresholdQuantity);
        SetUpdatedAt();
    }

    private StockAlertSeverity DetermineSeverity(StockAlertType type, int current, int threshold)
    {
        return type switch
        {
            StockAlertType.LowStock => current <= 0 ? StockAlertSeverity.Critical :
                                       current <= threshold * 0.5 ? StockAlertSeverity.High :
                                       StockAlertSeverity.Medium,
            StockAlertType.OutOfStock => StockAlertSeverity.Critical,
            StockAlertType.Overstock => current > threshold * 2 ? StockAlertSeverity.High :
                                       StockAlertSeverity.Medium,
            StockAlertType.ExpiringSoon => StockAlertSeverity.High,
            StockAlertType.Expired => StockAlertSeverity.Critical,
            _ => StockAlertSeverity.Low
        };
    }

    public int DaysActive => (DateTime.UtcNow - TriggeredDate).Days;
    public bool IsOverdue => DaysActive > 7 && Status == StockAlertStatus.Active;
}

public enum StockAlertType
{
    LowStock,
    OutOfStock,
    Overstock,
    ExpiringSoon,
    Expired,
    ReorderNeeded,
    StockVariance
}

public enum StockAlertSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum StockAlertStatus
{
    Active,
    Acknowledged,
    Resolved,
    Dismissed
}
