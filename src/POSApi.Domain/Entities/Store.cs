using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Store : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public TimeOnly OpenTime { get; private set; }
    public TimeOnly CloseTime { get; private set; }
    public string TimeZone { get; private set; } = string.Empty;
    public decimal TaxRate { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Guid ManagerUserId { get; private set; }
    public User Manager { get; private set; } = null!;
    public string Notes { get; private set; } = string.Empty;

    // Multi-Store/Branch Management Properties
    public StoreType StoreType { get; private set; }
    public Guid? ParentStoreId { get; private set; }
    public Store? ParentStore { get; private set; }

    public ICollection<StoreUser> StoreUsers { get; private set; } = new List<StoreUser>();
    public ICollection<StoreProduct> StoreProducts { get; private set; } = new List<StoreProduct>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    public ICollection<Store> ChildStores { get; private set; } = new List<Store>();
    public ICollection<StoreInventory> StoreInventories { get; private set; } = new List<StoreInventory>();
    public ICollection<InterStoreTransfer> TransfersFrom { get; private set; } = new List<InterStoreTransfer>();
    public ICollection<InterStoreTransfer> TransfersTo { get; private set; } = new List<InterStoreTransfer>();

    private Store() { } // For EF Core

    public Store(string name, string code, string address, string city, string state, string zipCode,
                string country, Guid managerUserId, StoreType storeType = StoreType.Branch,
                Guid? parentStoreId = null, string phoneNumber = "", string email = "")
    {
        Name = name;
        Code = code;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        ManagerUserId = managerUserId;
        PhoneNumber = phoneNumber;
        Email = email;
        IsActive = true;
        OpenTime = new TimeOnly(9, 0); // Default 9 AM
        CloseTime = new TimeOnly(17, 0); // Default 5 PM
        TimeZone = "UTC";
        Currency = "USD";
        TaxRate = 0.08m; // Default 8%
        StoreType = storeType;
        ParentStoreId = parentStoreId;

        AddDomainEvent(new StoreCreatedEvent(Id, name, code));
    }

    public void UpdateDetails(string name, string address, string city, string state, string zipCode, 
                             string phoneNumber, string email, string notes = "")
    {
        Name = name;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        PhoneNumber = phoneNumber;
        Email = email;
        Notes = notes;
        SetUpdatedAt();
    }

    public void UpdateOperatingHours(TimeOnly openTime, TimeOnly closeTime, string timeZone)
    {
        if (closeTime <= openTime)
        {
            throw new InvalidOperationException("Close time must be after open time");
        }

        OpenTime = openTime;
        CloseTime = closeTime;
        TimeZone = timeZone;
        SetUpdatedAt();
    }

    public void UpdateFinancialSettings(decimal taxRate, string currency)
    {
        if (taxRate < 0 || taxRate > 1)
        {
            throw new InvalidOperationException("Tax rate must be between 0 and 1");
        }

        TaxRate = taxRate;
        Currency = currency;
        SetUpdatedAt();
    }

    public void AssignManager(Guid managerUserId)
    {
        ManagerUserId = managerUserId;
        SetUpdatedAt();

        AddDomainEvent(new StoreManagerAssignedEvent(Id, Name, managerUserId));
    }

    public void AddUser(Guid userId, StoreRole role)
    {
        var existingUser = StoreUsers.FirstOrDefault(su => su.UserId == userId);
        if (existingUser != null)
        {
            existingUser.UpdateRole(role);
        }
        else
        {
            var storeUser = new StoreUser(Id, userId, role);
            StoreUsers.Add(storeUser);
        }

        SetUpdatedAt();
        AddDomainEvent(new StoreUserAssignedEvent(Id, Name, userId, role));
    }

    public void RemoveUser(Guid userId)
    {
        var storeUser = StoreUsers.FirstOrDefault(su => su.UserId == userId);
        if (storeUser != null)
        {
            StoreUsers.Remove(storeUser);
            SetUpdatedAt();

            AddDomainEvent(new StoreUserRemovedEvent(Id, Name, userId));
        }
    }

    public void AddProduct(Guid productId, decimal? storePrice = null, int stockQuantity = 0)
    {
        var existingProduct = StoreProducts.FirstOrDefault(sp => sp.ProductId == productId);
        if (existingProduct == null)
        {
            var storeProduct = new StoreProduct(Id, productId, storePrice, stockQuantity);
            StoreProducts.Add(storeProduct);
            SetUpdatedAt();
        }
    }

    public void UpdateProductStock(Guid productId, int stockQuantity)
    {
        var storeProduct = StoreProducts.FirstOrDefault(sp => sp.ProductId == productId);
        if (storeProduct != null)
        {
            storeProduct.UpdateStock(stockQuantity);
            SetUpdatedAt();
        }
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();

        AddDomainEvent(new StoreActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();

        AddDomainEvent(new StoreDeactivatedEvent(Id, Name));
    }

    public void UpdateManager(Guid managerUserId)
    {
        ManagerUserId = managerUserId;
        SetUpdatedAt();

        AddDomainEvent(new StoreManagerAssignedEvent(Id, Name, managerUserId));
    }

    public void SetParentStore(Guid? parentStoreId)
    {
        if (parentStoreId.HasValue && parentStoreId.Value == Id)
        {
            throw new InvalidOperationException("A store cannot be its own parent");
        }

        ParentStoreId = parentStoreId;
        SetUpdatedAt();
    }

    public void UpdateStoreType(StoreType storeType)
    {
        StoreType = storeType;
        SetUpdatedAt();
    }

    public bool IsOpenNow()
    {
        var now = TimeOnly.FromDateTime(DateTime.UtcNow);
        return IsActive && now >= OpenTime && now <= CloseTime;
    }

    public string FullAddress => $"{Address}, {City}, {State} {ZipCode}, {Country}";
}

public class StoreUser : BaseEntity
{
    public Guid StoreId { get; private set; }
    public Store Store { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public StoreRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime AssignedDate { get; private set; }

    private StoreUser() { } // For EF Core

    public StoreUser(Guid storeId, Guid userId, StoreRole role)
    {
        StoreId = storeId;
        UserId = userId;
        Role = role;
        IsActive = true;
        AssignedDate = DateTime.UtcNow;
    }

    public void UpdateRole(StoreRole role)
    {
        Role = role;
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
}

public class StoreProduct : BaseEntity
{
    public Guid StoreId { get; private set; }
    public Store Store { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public decimal? StorePrice { get; private set; }
    public int StockQuantity { get; private set; }
    public int MinStockLevel { get; private set; }
    public bool IsAvailable { get; private set; }
    public string BinLocation { get; private set; } = string.Empty;

    private StoreProduct() { } // For EF Core

    public StoreProduct(Guid storeId, Guid productId, decimal? storePrice = null, int stockQuantity = 0)
    {
        StoreId = storeId;
        ProductId = productId;
        StorePrice = storePrice;
        StockQuantity = stockQuantity;
        IsAvailable = true;
        MinStockLevel = 0;
    }

    public void UpdatePrice(decimal? storePrice)
    {
        StorePrice = storePrice;
        SetUpdatedAt();
    }

    public void UpdateStock(int stockQuantity)
    {
        StockQuantity = stockQuantity;
        SetUpdatedAt();
    }

    public void UpdateStockLevels(int minStockLevel)
    {
        MinStockLevel = minStockLevel;
        SetUpdatedAt();
    }

    public void UpdateBinLocation(string binLocation)
    {
        BinLocation = binLocation;
        SetUpdatedAt();
    }

    public void MakeAvailable()
    {
        IsAvailable = true;
        SetUpdatedAt();
    }

    public void MakeUnavailable()
    {
        IsAvailable = false;
        SetUpdatedAt();
    }

    public bool IsLowStock => StockQuantity <= MinStockLevel && MinStockLevel > 0;
    public decimal EffectivePrice(decimal basePrice) => StorePrice ?? basePrice;
}

public enum StoreRole
{
    Manager,
    AssistantManager,
    Cashier,
    StockClerk,
    Security,
    Maintenance
}

public enum StoreType
{
    Headquarter,
    Branch,
    Franchise,
    Warehouse
}

// StoreInventory - Junction entity with enhanced inventory management
public class StoreInventory : BaseEntity
{
    public Guid StoreId { get; private set; }
    public Store Store { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public int MinStockLevel { get; private set; }
    public int MaxStockLevel { get; private set; }
    public int ReorderPoint { get; private set; }
    public DateTime? LastRestockedAt { get; private set; }
    public DateTime? LastSoldAt { get; private set; }

    private StoreInventory() { } // For EF Core

    public StoreInventory(Guid storeId, Guid productId, int quantity = 0,
        int minStockLevel = 0, int maxStockLevel = 0, int reorderPoint = 0)
    {
        StoreId = storeId;
        ProductId = productId;
        Quantity = quantity;
        MinStockLevel = minStockLevel;
        MaxStockLevel = maxStockLevel;
        ReorderPoint = reorderPoint > 0 ? reorderPoint : minStockLevel;
    }

    public void AdjustQuantity(int amount)
    {
        var newQuantity = Quantity + amount;
        if (newQuantity < 0)
        {
            throw new InvalidOperationException($"Insufficient inventory. Available: {Quantity}, Requested: {Math.Abs(amount)}");
        }

        Quantity = newQuantity;

        if (amount > 0)
        {
            LastRestockedAt = DateTime.UtcNow;
        }
        else if (amount < 0)
        {
            LastSoldAt = DateTime.UtcNow;
        }

        SetUpdatedAt();
    }

    public void UpdateStockLevels(int minStockLevel, int maxStockLevel, int reorderPoint)
    {
        if (maxStockLevel > 0 && minStockLevel > maxStockLevel)
        {
            throw new InvalidOperationException("Minimum stock level cannot exceed maximum stock level");
        }

        MinStockLevel = minStockLevel;
        MaxStockLevel = maxStockLevel;
        ReorderPoint = reorderPoint;
        SetUpdatedAt();
    }

    public bool IsLowStock => Quantity <= MinStockLevel && MinStockLevel > 0;
    public bool NeedsReorder => Quantity <= ReorderPoint && ReorderPoint > 0;
    public bool IsOutOfStock => Quantity <= 0;
    public bool IsOverstocked => MaxStockLevel > 0 && Quantity > MaxStockLevel;
}

public enum InterStoreTransferStatus
{
    Draft,
    Pending,
    Approved,
    InTransit,
    Completed,
    Rejected,
    Cancelled
}

// InterStoreTransfer - Transfer between stores
public class InterStoreTransfer : AggregateRoot
{
    public string TransferNumber { get; private set; } = string.Empty;
    public Guid FromStoreId { get; private set; }
    public Store FromStore { get; private set; } = null!;
    public Guid ToStoreId { get; private set; }
    public Store ToStore { get; private set; } = null!;
    public Guid RequestedByUserId { get; private set; }
    public User RequestedBy { get; private set; } = null!;
    public Guid? ApprovedByUserId { get; private set; }
    public User? ApprovedBy { get; private set; }
    public InterStoreTransferStatus Status { get; private set; }
    public DateTime RequestDate { get; private set; }
    public DateTime? ApprovalDate { get; private set; }
    public DateTime? ShipDate { get; private set; }
    public DateTime? ReceiveDate { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public string? RejectionReason { get; private set; }
    public string? CancellationReason { get; private set; }

    public ICollection<InterStoreTransferItem> TransferItems { get; private set; } = new List<InterStoreTransferItem>();

    private InterStoreTransfer() { } // For EF Core

    public InterStoreTransfer(string transferNumber, Guid fromStoreId, Guid toStoreId,
        Guid requestedByUserId, string notes = "")
    {
        if (fromStoreId == toStoreId)
        {
            throw new InvalidOperationException("Cannot transfer to the same store");
        }

        TransferNumber = transferNumber;
        FromStoreId = fromStoreId;
        ToStoreId = toStoreId;
        RequestedByUserId = requestedByUserId;
        Status = InterStoreTransferStatus.Draft;
        RequestDate = DateTime.UtcNow;
        Notes = notes;
    }

    public void AddItem(Guid productId, int requestedQuantity, decimal unitCost)
    {
        if (Status != InterStoreTransferStatus.Draft)
        {
            throw new InvalidOperationException("Can only add items to draft transfers");
        }

        if (requestedQuantity <= 0)
        {
            throw new InvalidOperationException("Requested quantity must be greater than 0");
        }

        var existingItem = TransferItems.FirstOrDefault(i => i.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantities(requestedQuantity, requestedQuantity, 0, 0);
        }
        else
        {
            var item = new InterStoreTransferItem(Id, productId, requestedQuantity, unitCost);
            TransferItems.Add(item);
        }

        SetUpdatedAt();
    }

    public void Submit()
    {
        if (Status != InterStoreTransferStatus.Draft)
        {
            throw new InvalidOperationException($"Cannot submit transfer in {Status} status");
        }

        if (!TransferItems.Any())
        {
            throw new InvalidOperationException("Cannot submit transfer without items");
        }

        Status = InterStoreTransferStatus.Pending;
        SetUpdatedAt();
    }

    public void Approve(Guid approvedByUserId)
    {
        if (Status != InterStoreTransferStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot approve transfer in {Status} status");
        }

        ApprovedByUserId = approvedByUserId;
        ApprovalDate = DateTime.UtcNow;
        Status = InterStoreTransferStatus.Approved;

        // Set approved quantities equal to requested quantities by default
        foreach (var item in TransferItems)
        {
            item.UpdateQuantities(item.RequestedQuantity, item.RequestedQuantity, 0, 0);
        }

        SetUpdatedAt();
    }

    public void Reject(Guid rejectedByUserId, string reason)
    {
        if (Status != InterStoreTransferStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot reject transfer in {Status} status");
        }

        ApprovedByUserId = rejectedByUserId;
        ApprovalDate = DateTime.UtcNow;
        RejectionReason = reason;
        Status = InterStoreTransferStatus.Rejected;
        SetUpdatedAt();
    }

    public void Ship()
    {
        if (Status != InterStoreTransferStatus.Approved)
        {
            throw new InvalidOperationException($"Cannot ship transfer in {Status} status");
        }

        ShipDate = DateTime.UtcNow;
        Status = InterStoreTransferStatus.InTransit;

        // Set shipped quantities equal to approved quantities
        foreach (var item in TransferItems)
        {
            item.UpdateQuantities(item.RequestedQuantity, item.ApprovedQuantity,
                item.ApprovedQuantity, 0);
        }

        SetUpdatedAt();
    }

    public void Complete()
    {
        if (Status != InterStoreTransferStatus.InTransit)
        {
            throw new InvalidOperationException($"Cannot complete transfer in {Status} status");
        }

        ReceiveDate = DateTime.UtcNow;
        Status = InterStoreTransferStatus.Completed;

        // Set received quantities equal to shipped quantities if not already set
        foreach (var item in TransferItems)
        {
            if (item.ReceivedQuantity == 0)
            {
                item.UpdateQuantities(item.RequestedQuantity, item.ApprovedQuantity,
                    item.ShippedQuantity, item.ShippedQuantity);
            }
        }

        SetUpdatedAt();
    }

    public void Cancel(string reason)
    {
        if (Status == InterStoreTransferStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel completed transfer");
        }

        if (Status == InterStoreTransferStatus.InTransit)
        {
            throw new InvalidOperationException("Cannot cancel transfer that is already in transit");
        }

        CancellationReason = reason;
        Status = InterStoreTransferStatus.Cancelled;
        SetUpdatedAt();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }
}

// InterStoreTransferItem - Individual items in a transfer
public class InterStoreTransferItem : BaseEntity
{
    public Guid TransferId { get; private set; }
    public InterStoreTransfer Transfer { get; private set; } = null!;
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int RequestedQuantity { get; private set; }
    public int ApprovedQuantity { get; private set; }
    public int ShippedQuantity { get; private set; }
    public int ReceivedQuantity { get; private set; }
    public decimal UnitCost { get; private set; }

    private InterStoreTransferItem() { } // For EF Core

    public InterStoreTransferItem(Guid transferId, Guid productId, int requestedQuantity, decimal unitCost)
    {
        TransferId = transferId;
        ProductId = productId;
        RequestedQuantity = requestedQuantity;
        ApprovedQuantity = 0;
        ShippedQuantity = 0;
        ReceivedQuantity = 0;
        UnitCost = unitCost;
    }

    public void UpdateQuantities(int requested, int approved, int shipped, int received)
    {
        RequestedQuantity = requested;
        ApprovedQuantity = approved;
        ShippedQuantity = shipped;
        ReceivedQuantity = received;
        SetUpdatedAt();
    }

    public void UpdateUnitCost(decimal unitCost)
    {
        UnitCost = unitCost;
        SetUpdatedAt();
    }

    public decimal TotalCost => ReceivedQuantity > 0 ? ReceivedQuantity * UnitCost :
                                 ShippedQuantity > 0 ? ShippedQuantity * UnitCost :
                                 ApprovedQuantity > 0 ? ApprovedQuantity * UnitCost :
                                 RequestedQuantity * UnitCost;
}