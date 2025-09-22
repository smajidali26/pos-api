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

    public ICollection<StoreUser> StoreUsers { get; private set; } = new List<StoreUser>();
    public ICollection<StoreProduct> StoreProducts { get; private set; } = new List<StoreProduct>();
    public ICollection<Order> Orders { get; private set; } = new List<Order>();

    private Store() { } // For EF Core

    public Store(string name, string code, string address, string city, string state, string zipCode, 
                string country, Guid managerUserId, string phoneNumber = "", string email = "")
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