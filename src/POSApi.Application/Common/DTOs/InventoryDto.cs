using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class InventoryMovementDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public MovementType MovementType { get; set; }
    public string MovementTypeName { get; set; } = string.Empty;
    public int PreviousQuantity { get; set; }
    public int QuantityChanged { get; set; }
    public int NewQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public Guid? ReferenceTransactionId { get; set; }
    public Guid PerformedByUserId { get; set; }
    public string PerformedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public LocationType LocationType { get; set; }
    public string LocationTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? ParentLocationId { get; set; }
    public string? ParentLocationName { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductLocationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid LocationId { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int? MinStockLevel { get; set; }
    public int? MaxStockLevel { get; set; }
    public int? ReorderPoint { get; set; }
    public string? BinLocation { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsOverStock { get; set; }
    public DateTime? LastRestockedDate { get; set; }
    public DateTime? LastMovementDate { get; set; }
}

public class StockTransferDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid FromLocationId { get; set; }
    public string FromLocationName { get; set; } = string.Empty;
    public Guid ToLocationId { get; set; }
    public string ToLocationName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int ShippedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public int Variance { get; set; }
    public bool HasVariance { get; set; }
    public TransferStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid RequestedByUserId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public Guid? ShippedByUserId { get; set; }
    public string? ShippedByName { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string? TrackingNumber { get; set; }
    public Guid? ReceivedByUserId { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }
    public decimal? ShippingCost { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BatchDto
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int InitialQuantity { get; set; }
    public int CurrentQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? ManufactureDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public int? DaysUntilExpiry { get; set; }
    public BatchStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? RecallReason { get; set; }
    public Guid? RecalledByUserId { get; set; }
    public string? RecalledByName { get; set; }
    public DateTime? RecalledDate { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SerialNumberDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public SerialNumberStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public DateTime? SoldDate { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public int? WarrantyMonths { get; set; }
    public bool IsUnderWarranty { get; set; }
    public int? DaysRemainingInWarranty { get; set; }
    public string? Notes { get; set; }
    public List<SerialNumberHistoryDto> History { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SerialNumberHistoryDto
{
    public Guid Id { get; set; }
    public Guid SerialNumberId { get; set; }
    public SerialNumberHistoryAction Action { get; set; }
    public string ActionName { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string? NewStatus { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByName { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StockAlertDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public StockAlertType AlertType { get; set; }
    public string AlertTypeName { get; set; } = string.Empty;
    public StockAlertSeverity Severity { get; set; }
    public string SeverityName { get; set; } = string.Empty;
    public StockAlertStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int CurrentQuantity { get; set; }
    public int ThresholdQuantity { get; set; }
    public DateTime TriggeredDate { get; set; }
    public Guid? AcknowledgedByUserId { get; set; }
    public string? AcknowledgedByName { get; set; }
    public DateTime? AcknowledgedDate { get; set; }
    public string? AcknowledgmentNotes { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolvedByName { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? DismissReason { get; set; }
    public int DaysActive { get; set; }
    public bool IsOverdue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InventoryValuationDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public ValuationMethod Method { get; set; }
    public string MethodName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal AverageCost { get; set; }
    public decimal TotalValue { get; set; }
    public DateTime? LastValuationDate { get; set; }
    public List<InventoryValuationLayerDto> Layers { get; set; } = new();
}

public class InventoryValuationLayerDto
{
    public Guid Id { get; set; }
    public Guid InventoryValuationId { get; set; }
    public int LayerSequence { get; set; }
    public decimal UnitCost { get; set; }
    public int InitialQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public int ConsumedQuantity { get; set; }
    public decimal TotalValue { get; set; }
    public decimal RemainingValue { get; set; }
    public DateTime AcquiredDate { get; set; }
}

// Summary/Statistics DTOs
public class InventorySummaryDto
{
    public int TotalProducts { get; set; }
    public int TotalLocations { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int OverStockCount { get; set; }
    public decimal TotalInventoryValue { get; set; }
}

public class StockTransferStatisticsDto
{
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int InTransitCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public int TotalWithVariance { get; set; }
    public decimal AverageTransferTime { get; set; }
}

public class StockAlertSummaryDto
{
    public int TotalActiveAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int HighPriorityAlerts { get; set; }
    public int MediumPriorityAlerts { get; set; }
    public int LowPriorityAlerts { get; set; }
    public int OverdueAlerts { get; set; }
    public int OutOfStockAlerts { get; set; }
    public int LowStockAlerts { get; set; }
    public int ExpiringAlerts { get; set; }
}

public class BatchStatisticsDto
{
    public int TotalActiveBatches { get; set; }
    public int ExpiredBatches { get; set; }
    public int ExpiringSoonBatches { get; set; }
    public int RecalledBatches { get; set; }
    public int DepletedBatches { get; set; }
    public decimal TotalBatchValue { get; set; }
}
