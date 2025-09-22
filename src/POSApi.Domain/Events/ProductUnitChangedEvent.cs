using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class ProductUnitChangedEvent : DomainEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public ProductUnit OldUnit { get; }
    public ProductUnit NewUnit { get; }

    public ProductUnitChangedEvent(Guid productId, string productName, ProductUnit oldUnit, ProductUnit newUnit)
    {
        ProductId = productId;
        ProductName = productName;
        OldUnit = oldUnit;
        NewUnit = newUnit;
    }
}