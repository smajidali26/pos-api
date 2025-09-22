using POSApi.Domain.Common;

namespace POSApi.Domain.Events;

public class ProductPriceChangedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public ProductPriceChangedEvent(Guid productId, decimal oldPrice, decimal newPrice)
    {
        ProductId = productId;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}