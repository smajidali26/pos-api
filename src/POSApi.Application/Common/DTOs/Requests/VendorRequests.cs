using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Requests;

public class UpdateVendorStatusRequest
{
    public VendorStatus Status { get; set; }
    public string? Reason { get; set; }
}

public class BlockVendorRequest
{
    public string Reason { get; set; } = string.Empty;
}
