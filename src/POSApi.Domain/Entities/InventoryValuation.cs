using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

public class InventoryValuation : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid? LocationId { get; private set; }
    public Location? Location { get; private set; }
    public ValuationMethod Method { get; private set; }
    public int TotalQuantity { get; private set; }
    public decimal TotalValue { get; private set; }
    public decimal AverageCost { get; private set; }
    public DateTime ValuationDate { get; private set; }
    public Guid CalculatedByUserId { get; private set; }
    public User CalculatedBy { get; private set; } = null!;
    public string Notes { get; private set; } = string.Empty;

    public ICollection<InventoryValuationLayer> ValuationLayers { get; private set; } = new List<InventoryValuationLayer>();

    private InventoryValuation() { } // For EF Core

    public InventoryValuation(Guid productId, ValuationMethod method, Guid calculatedByUserId, Guid? locationId = null, string notes = "")
    {
        ProductId = productId;
        Method = method;
        CalculatedByUserId = calculatedByUserId;
        LocationId = locationId;
        Notes = notes;
        ValuationDate = DateTime.UtcNow;
        TotalQuantity = 0;
        TotalValue = 0;
        AverageCost = 0;
    }

    public void AddLayer(int quantity, decimal unitCost, DateTime receivedDate, Guid? batchId = null, string reference = "")
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (unitCost < 0)
        {
            throw new ArgumentException("Unit cost cannot be negative");
        }

        var layer = new InventoryValuationLayer(Id, quantity, unitCost, receivedDate, batchId, reference);
        ValuationLayers.Add(layer);

        RecalculateTotals();
        SetUpdatedAt();
    }

    public void ConsumeQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (quantity > TotalQuantity)
        {
            throw new InvalidOperationException($"Insufficient quantity. Available: {TotalQuantity}, Requested: {quantity}");
        }

        var remainingToConsume = quantity;

        // Get layers in order based on valuation method
        var layersToConsume = Method switch
        {
            ValuationMethod.FIFO => ValuationLayers.Where(l => l.RemainingQuantity > 0).OrderBy(l => l.ReceivedDate).ToList(),
            ValuationMethod.LIFO => ValuationLayers.Where(l => l.RemainingQuantity > 0).OrderByDescending(l => l.ReceivedDate).ToList(),
            ValuationMethod.WeightedAverage => ValuationLayers.Where(l => l.RemainingQuantity > 0).OrderBy(l => l.ReceivedDate).ToList(), // Order doesn't matter for weighted average
            _ => throw new InvalidOperationException($"Unsupported valuation method: {Method}")
        };

        foreach (var layer in layersToConsume)
        {
            if (remainingToConsume <= 0) break;

            var quantityToConsume = Math.Min(remainingToConsume, layer.RemainingQuantity);
            layer.ConsumeQuantity(quantityToConsume);
            remainingToConsume -= quantityToConsume;
        }

        RecalculateTotals();
        SetUpdatedAt();
    }

    public decimal CalculateCOGS(int quantity)
    {
        if (quantity <= 0 || quantity > TotalQuantity)
        {
            throw new ArgumentException("Invalid quantity for COGS calculation");
        }

        if (Method == ValuationMethod.WeightedAverage)
        {
            return quantity * AverageCost;
        }

        var cogs = 0m;
        var remainingQuantity = quantity;

        var layersForCOGS = Method switch
        {
            ValuationMethod.FIFO => ValuationLayers.Where(l => l.RemainingQuantity > 0).OrderBy(l => l.ReceivedDate).ToList(),
            ValuationMethod.LIFO => ValuationLayers.Where(l => l.RemainingQuantity > 0).OrderByDescending(l => l.ReceivedDate).ToList(),
            _ => new List<InventoryValuationLayer>()
        };

        foreach (var layer in layersForCOGS)
        {
            if (remainingQuantity <= 0) break;

            var quantityFromLayer = Math.Min(remainingQuantity, layer.RemainingQuantity);
            cogs += quantityFromLayer * layer.UnitCost;
            remainingQuantity -= quantityFromLayer;
        }

        return cogs;
    }

    private void RecalculateTotals()
    {
        TotalQuantity = ValuationLayers.Sum(l => l.RemainingQuantity);
        TotalValue = ValuationLayers.Sum(l => l.RemainingValue);
        AverageCost = TotalQuantity > 0 ? TotalValue / TotalQuantity : 0;
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }
}

public class InventoryValuationLayer : BaseEntity
{
    public Guid InventoryValuationId { get; private set; }
    public InventoryValuation InventoryValuation { get; private set; } = null!;
    public int InitialQuantity { get; private set; }
    public int RemainingQuantity { get; private set; }
    public int ConsumedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public decimal RemainingValue { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public Guid? BatchId { get; private set; }
    public Batch? Batch { get; private set; }
    public string Reference { get; private set; } = string.Empty;

    private InventoryValuationLayer() { } // For EF Core

    public InventoryValuationLayer(Guid inventoryValuationId, int quantity, decimal unitCost, DateTime receivedDate, Guid? batchId = null, string reference = "")
    {
        InventoryValuationId = inventoryValuationId;
        InitialQuantity = quantity;
        RemainingQuantity = quantity;
        ConsumedQuantity = 0;
        UnitCost = unitCost;
        TotalCost = quantity * unitCost;
        RemainingValue = TotalCost;
        ReceivedDate = receivedDate;
        BatchId = batchId;
        Reference = reference;
    }

    public void ConsumeQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero");
        }

        if (quantity > RemainingQuantity)
        {
            throw new InvalidOperationException($"Cannot consume more than remaining quantity. Available: {RemainingQuantity}, Requested: {quantity}");
        }

        RemainingQuantity -= quantity;
        ConsumedQuantity += quantity;
        RemainingValue = RemainingQuantity * UnitCost;
        SetUpdatedAt();
    }

    public bool IsFullyConsumed => RemainingQuantity == 0;
    public decimal ConsumedValue => ConsumedQuantity * UnitCost;
}

public enum ValuationMethod
{
    FIFO,           // First In, First Out
    LIFO,           // Last In, First Out
    WeightedAverage // Weighted Average Cost
}
