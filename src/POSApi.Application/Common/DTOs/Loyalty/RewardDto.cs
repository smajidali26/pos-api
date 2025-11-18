using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Loyalty;

public class RewardDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public string RewardType { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public int? MaxRedemptionsPerCustomer { get; set; }
    public int? TotalRedemptionsAllowed { get; set; }
    public int CurrentRedemptions { get; set; }
    public bool IsActive { get; set; }
    public bool IsAvailable { get; set; }
    public int? CustomerRedemptions { get; set; }
}
