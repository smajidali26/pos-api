using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class LoyaltyPointsRedeemedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public decimal PointsRedeemed { get; }
    public decimal RemainingPoints { get; }

    public LoyaltyPointsRedeemedEvent(Guid customerId, decimal pointsRedeemed, decimal remainingPoints)
    {
        CustomerId = customerId;
        PointsRedeemed = pointsRedeemed;
        RemainingPoints = remainingPoints;
    }
}