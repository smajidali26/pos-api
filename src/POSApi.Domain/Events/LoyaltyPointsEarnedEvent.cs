using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class LoyaltyPointsEarnedEvent : DomainEvent
{
    public Guid CustomerId { get; }
    public decimal PointsEarned { get; }
    public decimal TotalPoints { get; }

    public LoyaltyPointsEarnedEvent(Guid customerId, decimal pointsEarned, decimal totalPoints)
    {
        CustomerId = customerId;
        PointsEarned = pointsEarned;
        TotalPoints = totalPoints;
    }
}