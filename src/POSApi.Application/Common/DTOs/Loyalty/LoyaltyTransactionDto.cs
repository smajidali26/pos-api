using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Loyalty;

public class LoyaltyTransactionDto
{
    public Guid Id { get; set; }
    public Guid CustomerLoyaltyId { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public int PointsEarned { get; set; }
    public int PointsRedeemed { get; set; }
    public int BalanceBefore { get; set; }
    public int BalanceAfter { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
}
