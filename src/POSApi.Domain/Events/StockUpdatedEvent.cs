using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class StockUpdatedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public int OldQuantity { get; }
    public int NewQuantity { get; }

    public StockUpdatedEvent(Guid productId, int oldQuantity, int newQuantity)
    {
        ProductId = productId;
        OldQuantity = oldQuantity;
        NewQuantity = newQuantity;
    }
}