using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class PromotionCreatedEvent : DomainEvent
{
    public Guid PromotionId { get; }
    public string PromotionName { get; }
    public PromotionType Type { get; }

    public PromotionCreatedEvent(Guid promotionId, string promotionName, PromotionType type)
    {
        PromotionId = promotionId;
        PromotionName = promotionName;
        Type = type;
    }
}

public class PromotionUsedEvent : DomainEvent
{
    public Guid PromotionId { get; }
    public Guid OrderId { get; }
    public decimal DiscountAmount { get; }
    public string PromotionName { get; }

    public PromotionUsedEvent(Guid promotionId, Guid orderId, decimal discountAmount, string promotionName)
    {
        PromotionId = promotionId;
        OrderId = orderId;
        DiscountAmount = discountAmount;
        PromotionName = promotionName;
    }
}

public class PromotionActivatedEvent : DomainEvent
{
    public Guid PromotionId { get; }
    public string PromotionName { get; }

    public PromotionActivatedEvent(Guid promotionId, string promotionName)
    {
        PromotionId = promotionId;
        PromotionName = promotionName;
    }
}

public class PromotionDeactivatedEvent : DomainEvent
{
    public Guid PromotionId { get; }
    public string PromotionName { get; }

    public PromotionDeactivatedEvent(Guid promotionId, string promotionName)
    {
        PromotionId = promotionId;
        PromotionName = promotionName;
    }
}

public class PromotionExpiredEvent : DomainEvent
{
    public Guid PromotionId { get; }
    public string PromotionName { get; }
    public DateTime ExpiredAt { get; }

    public PromotionExpiredEvent(Guid promotionId, string promotionName, DateTime expiredAt)
    {
        PromotionId = promotionId;
        PromotionName = promotionName;
        ExpiredAt = expiredAt;
    }
}