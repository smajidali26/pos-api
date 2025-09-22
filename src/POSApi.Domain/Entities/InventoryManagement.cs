using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class InventoryMovement : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public MovementType Type { get; private set; }
    public int Quantity { get; private set; }
    public int PreviousQuantity { get; private set; }
    public int NewQuantity { get; private set; }
    public decimal? UnitCost { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string ReferenceNumber { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public Guid MovedByUserId { get; private set; }
    public User MovedBy { get; private set; } = null!;
    public DateTime MovementDate { get; private set; }
    public Guid? LocationId { get; private set; }
    public Location? Location { get; private set; }

    private InventoryMovement() { } // For EF Core

    public InventoryMovement(Guid productId, MovementType type, int quantity, int previousQuantity, 
                           Guid movedByUserId, string reason, string referenceNumber = "", 
                           Guid? referenceId = null, decimal? unitCost = null, Guid? locationId = null)
    {
        ProductId = productId;
        Type = type;
        Quantity = Math.Abs(quantity);
        PreviousQuantity = previousQuantity;
        MovedByUserId = movedByUserId;
        Reason = reason;
        ReferenceNumber = referenceNumber;
        ReferenceId = referenceId;
        UnitCost = unitCost;
        LocationId = locationId;
        MovementDate = DateTime.UtcNow;

        NewQuantity = type switch
        {
            MovementType.StockIn => previousQuantity + Quantity,
            MovementType.StockOut => previousQuantity - Quantity,
            MovementType.Adjustment => quantity, // quantity can be negative for adjustments
            MovementType.Transfer => previousQuantity - Quantity,
            _ => previousQuantity
        };

        AddDomainEvent(new InventoryMovementRecordedEvent(Id, ProductId, type, Quantity, NewQuantity));
    }
}

public class Location : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public LocationType Type { get; private set; }
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public Guid? ParentLocationId { get; private set; }
    public Location? ParentLocation { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;

    public ICollection<Location> SubLocations { get; private set; } = new List<Location>();
    public ICollection<ProductLocation> ProductLocations { get; private set; } = new List<ProductLocation>();
    public ICollection<InventoryMovement> InventoryMovements { get; private set; } = new List<InventoryMovement>();

    private Location() { } // For EF Core

    public Location(string name, string code, LocationType type, Guid createdByUserId, 
                   string address = "", string city = "", string state = "", string zipCode = "")
    {
        Name = name;
        Code = code;
        Type = type;
        CreatedByUserId = createdByUserId;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        IsActive = true;

        AddDomainEvent(new LocationCreatedEvent(Id, name, code, type));
    }

    public void UpdateDetails(string name, string address, string city, string state, string zipCode, string notes = "")
    {
        Name = name;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        Notes = notes;
        SetUpdatedAt();
    }

    public void SetParentLocation(Guid? parentLocationId)
    {
        if (parentLocationId == Id)
        {
            throw new InvalidOperationException("Location cannot be its own parent");
        }

        ParentLocationId = parentLocationId;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public string FullAddress => $"{Address}, {City}, {State} {ZipCode}".Trim(' ', ',');
}

public class ProductLocation : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;
    public int Quantity { get; private set; }
    public int MinStockLevel { get; private set; }
    public int MaxStockLevel { get; private set; }
    public string BinLocation { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    private ProductLocation() { } // For EF Core

    public ProductLocation(Guid productId, Guid locationId, int quantity = 0, 
                          int minStockLevel = 0, int maxStockLevel = 0, string binLocation = "")
    {
        ProductId = productId;
        LocationId = locationId;
        Quantity = quantity;
        MinStockLevel = minStockLevel;
        MaxStockLevel = maxStockLevel;
        BinLocation = binLocation;
        IsActive = true;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative");
        }

        Quantity = newQuantity;
        SetUpdatedAt();
    }

    public void UpdateStockLevels(int minLevel, int maxLevel)
    {
        if (minLevel < 0 || maxLevel < 0)
        {
            throw new ArgumentException("Stock levels cannot be negative");
        }

        if (maxLevel > 0 && minLevel > maxLevel)
        {
            throw new ArgumentException("Minimum stock level cannot exceed maximum stock level");
        }

        MinStockLevel = minLevel;
        MaxStockLevel = maxLevel;
        SetUpdatedAt();
    }

    public void UpdateBinLocation(string binLocation)
    {
        BinLocation = binLocation;
        SetUpdatedAt();
    }

    public bool IsLowStock => Quantity <= MinStockLevel && MinStockLevel > 0;
    public bool IsOverStock => MaxStockLevel > 0 && Quantity > MaxStockLevel;
}

public class StockCount : AggregateRoot
{
    public string CountNumber { get; private set; } = string.Empty;
    public Guid LocationId { get; private set; }
    public Location Location { get; private set; } = null!;
    public StockCountStatus Status { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid CreatedByUserId { get; private set; }
    public User CreatedBy { get; private set; } = null!;
    public Guid? CountedByUserId { get; private set; }
    public User? CountedBy { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    public ICollection<StockCountItem> CountItems { get; private set; } = new List<StockCountItem>();

    private StockCount() { } // For EF Core

    public StockCount(string countNumber, Guid locationId, DateTime scheduledDate, Guid createdByUserId)
    {
        CountNumber = countNumber;
        LocationId = locationId;
        ScheduledDate = scheduledDate;
        CreatedByUserId = createdByUserId;
        Status = StockCountStatus.Scheduled;

        AddDomainEvent(new StockCountCreatedEvent(Id, countNumber, locationId));
    }

    public void Start(Guid countedByUserId)
    {
        if (Status != StockCountStatus.Scheduled)
        {
            throw new InvalidOperationException($"Cannot start stock count in {Status} status");
        }

        Status = StockCountStatus.InProgress;
        CountedByUserId = countedByUserId;
        StartedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void AddCountItem(Guid productId, int countedQuantity, int systemQuantity)
    {
        if (Status != StockCountStatus.InProgress)
        {
            throw new InvalidOperationException("Can only add items to in-progress stock counts");
        }

        var existingItem = CountItems.FirstOrDefault(ci => ci.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateCount(countedQuantity);
        }
        else
        {
            var countItem = new StockCountItem(Id, productId, countedQuantity, systemQuantity);
            CountItems.Add(countItem);
        }

        SetUpdatedAt();
    }

    public void Complete()
    {
        if (Status != StockCountStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot complete stock count in {Status} status");
        }

        Status = StockCountStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        SetUpdatedAt();

        AddDomainEvent(new StockCountCompletedEvent(Id, CountNumber, CountItems.Count));
    }

    public void Cancel(string reason)
    {
        if (Status == StockCountStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel completed stock count");
        }

        Status = StockCountStatus.Cancelled;
        Notes = $"Cancelled: {reason}. {Notes}";
        SetUpdatedAt();
    }

    public decimal VariancePercentage
    {
        get
        {
            if (!CountItems.Any()) return 0;
            
            var totalVariance = CountItems.Sum(ci => Math.Abs(ci.Variance));
            var totalSystemQuantity = CountItems.Sum(ci => ci.SystemQuantity);
            
            return totalSystemQuantity > 0 ? (totalVariance / totalSystemQuantity) * 100 : 0;
        }
    }
}

public class StockCountItem : BaseEntity
{
    public Guid StockCountId { get; private set; }
    public StockCount StockCount { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int CountedQuantity { get; private set; }
    public int SystemQuantity { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    private StockCountItem() { } // For EF Core

    public StockCountItem(Guid stockCountId, Guid productId, int countedQuantity, int systemQuantity)
    {
        StockCountId = stockCountId;
        ProductId = productId;
        CountedQuantity = countedQuantity;
        SystemQuantity = systemQuantity;
    }

    public void UpdateCount(int countedQuantity, string notes = "")
    {
        CountedQuantity = countedQuantity;
        Notes = notes;
        SetUpdatedAt();
    }

    public int Variance => CountedQuantity - SystemQuantity;
    public bool HasVariance => Variance != 0;
}

public enum MovementType
{
    StockIn,      // Receiving inventory
    StockOut,     // Sales, waste, etc.
    Adjustment,   // Manual adjustments
    Transfer,     // Moving between locations
    Return,       // Customer returns
    Damage,       // Damaged goods
    Theft,        // Theft/shrinkage
    Expiry        // Expired products
}

public enum LocationType
{
    Store,
    Warehouse,
    Backroom,
    Display,
    Shelf,
    Bin
}

public enum StockCountStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}