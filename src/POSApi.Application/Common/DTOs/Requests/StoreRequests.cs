using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Requests;

public class CreateStoreRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid ManagerUserId { get; set; }
    public StoreType StoreType { get; set; } = StoreType.Branch;
    public Guid? ParentStoreId { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Operating Hours
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public string? TimeZone { get; set; }

    // Financial Settings
    public decimal? TaxRate { get; set; }
    public string? Currency { get; set; }
}

public class UpdateStoreRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public StoreType? StoreType { get; set; }
    public Guid? ParentStoreId { get; set; }

    // Operating Hours
    public TimeOnly? OpenTime { get; set; }
    public TimeOnly? CloseTime { get; set; }
    public string? TimeZone { get; set; }

    // Financial Settings
    public decimal? TaxRate { get; set; }
    public string? Currency { get; set; }
}

public class UpdateStoreManagerRequest
{
    public Guid ManagerUserId { get; set; }
}

public class AdjustStoreInventoryRequest
{
    public Guid ProductId { get; set; }
    public int AdjustmentAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class UpdateStockLevelsRequest
{
    public Guid ProductId { get; set; }
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public int ReorderPoint { get; set; }
}
