using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class StoreDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string TimeZone { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid ManagerUserId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Multi-Store Properties
    public StoreType StoreType { get; set; }
    public Guid? ParentStoreId { get; set; }
    public string? ParentStoreName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed Properties
    public string FullAddress { get; set; } = string.Empty;
    public bool IsOpenNow { get; set; }
}

public class StoreHierarchyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public StoreType StoreType { get; set; }
    public Guid? ParentStoreId { get; set; }
    public bool IsActive { get; set; }
    public List<StoreHierarchyDto> ChildStores { get; set; } = new();
}

public class StoreInventoryDto
{
    public Guid Id { get; set; }
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public int ReorderPoint { get; set; }
    public DateTime? LastRestockedAt { get; set; }
    public DateTime? LastSoldAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed Properties
    public bool IsLowStock { get; set; }
    public bool NeedsReorder { get; set; }
    public bool IsOutOfStock { get; set; }
    public bool IsOverstocked { get; set; }
}

public class StoreSummaryDto
{
    public Guid StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreCode { get; set; } = string.Empty;
    public StoreType StoreType { get; set; }
    public bool IsActive { get; set; }

    // Inventory Summary
    public int TotalProducts { get; set; }
    public int LowStockProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }

    // Transfer Summary
    public int PendingIncomingTransfers { get; set; }
    public int PendingOutgoingTransfers { get; set; }
    public int InTransitIncoming { get; set; }
    public int InTransitOutgoing { get; set; }

    // Staff Summary
    public int TotalStaff { get; set; }
    public int ActiveStaff { get; set; }
}

public class InterStoreTransferDto
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;

    public Guid FromStoreId { get; set; }
    public string FromStoreName { get; set; } = string.Empty;
    public string FromStoreCode { get; set; } = string.Empty;

    public Guid ToStoreId { get; set; }
    public string ToStoreName { get; set; } = string.Empty;
    public string ToStoreCode { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }
    public string RequestedByName { get; set; } = string.Empty;

    public Guid? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }

    public TransferStatus Status { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public DateTime? ShipDate { get; set; }
    public DateTime? ReceiveDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }

    public List<InterStoreTransferItemDto> Items { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed Properties
    public int TotalItems { get; set; }
    public decimal TotalCost { get; set; }
}

public class InterStoreTransferItemDto
{
    public Guid Id { get; set; }
    public Guid TransferId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int ApprovedQuantity { get; set; }
    public int ShippedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
