namespace POSApi.Application.Common.DTOs.Loyalty;

public class CustomerTierDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal MinSpend { get; set; }
    public int MinPoints { get; set; }
    public decimal BenefitMultiplier { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string Color { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
