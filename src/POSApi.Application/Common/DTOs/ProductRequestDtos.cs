namespace POSApi.Application.Common.DTOs;

// Request DTOs for Product operations
public class UpdateProductPricingRequest
{
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}

public class UpdateStockRequest
{
    public int NewQuantity { get; set; }
    public string? Reason { get; set; }
}

public class UnitConversionRequest
{
    public decimal Quantity { get; set; }
    public string FromUnitCode { get; set; } = string.Empty;
    public string ToUnitCode { get; set; } = string.Empty;
}