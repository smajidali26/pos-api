using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Vendor : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string CompanyName { get; private set; } = string.Empty;
    public string ContactPerson { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string TaxId { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;
    public VendorType Type { get; private set; }
    public VendorStatus Status { get; private set; }
    public PaymentTerms PaymentTerms { get; private set; }
    public decimal CreditLimit { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public DateTime? LastOrderDate { get; private set; }
    public string Notes { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public ICollection<PurchaseOrder> PurchaseOrders { get; private set; } = new List<PurchaseOrder>();

    private Vendor() { } // For EF Core

    public Vendor(string name, string companyName, string contactPerson, string email, 
                 string phoneNumber, string address, string city, string state, 
                 string zipCode, string country, VendorType type = VendorType.Supplier)
    {
        Name = name;
        CompanyName = companyName;
        ContactPerson = contactPerson;
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Type = type;
        Status = VendorStatus.Active;
        PaymentTerms = PaymentTerms.Net30;
        CreditLimit = 0;
        CurrentBalance = 0;

        AddDomainEvent(new VendorCreatedEvent(Id, name, companyName, email));
    }

    public void UpdateContactInfo(string contactPerson, string email, string phoneNumber)
    {
        ContactPerson = contactPerson;
        Email = email;
        PhoneNumber = phoneNumber;
        SetUpdatedAt();

        AddDomainEvent(new VendorContactUpdatedEvent(Id, contactPerson, email, phoneNumber));
    }

    public void UpdateAddress(string address, string city, string state, string zipCode, string country)
    {
        Address = address;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        SetUpdatedAt();
    }

    public void UpdatePaymentTerms(PaymentTerms paymentTerms)
    {
        PaymentTerms = paymentTerms;
        SetUpdatedAt();

        AddDomainEvent(new VendorPaymentTermsUpdatedEvent(Id, paymentTerms));
    }

    public void UpdateCreditLimit(decimal creditLimit)
    {
        var oldLimit = CreditLimit;
        CreditLimit = creditLimit;
        SetUpdatedAt();

        AddDomainEvent(new VendorCreditLimitUpdatedEvent(Id, oldLimit, creditLimit));
    }

    public void UpdateBalance(decimal amount)
    {
        var oldBalance = CurrentBalance;
        CurrentBalance += amount;
        SetUpdatedAt();

        if (CurrentBalance > CreditLimit && CreditLimit > 0)
        {
            AddDomainEvent(new VendorCreditLimitExceededEvent(Id, Name, CurrentBalance, CreditLimit));
        }

        AddDomainEvent(new VendorBalanceUpdatedEvent(Id, oldBalance, CurrentBalance));
    }

    public void RecordOrder(DateTime orderDate, decimal orderAmount)
    {
        LastOrderDate = orderDate;
        UpdateBalance(orderAmount);
        SetUpdatedAt();

        AddDomainEvent(new VendorOrderRecordedEvent(Id, orderDate, orderAmount));
    }

    public void UpdateStatus(VendorStatus status)
    {
        var oldStatus = Status;
        Status = status;
        SetUpdatedAt();

        AddDomainEvent(new VendorStatusChangedEvent(Id, oldStatus, status));
    }

    public void UpdateTaxId(string taxId)
    {
        TaxId = taxId;
        SetUpdatedAt();
    }

    public void UpdateWebsite(string website)
    {
        Website = website;
        SetUpdatedAt();
    }

    public void UpdateNotes(string notes)
    {
        Notes = notes;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        Status = VendorStatus.Active;
        SetUpdatedAt();

        AddDomainEvent(new VendorActivatedEvent(Id, Name));
    }

    public void Deactivate()
    {
        IsActive = false;
        Status = VendorStatus.Inactive;
        SetUpdatedAt();

        AddDomainEvent(new VendorDeactivatedEvent(Id, Name));
    }

    public void Block(string reason)
    {
        Status = VendorStatus.Blocked;
        Notes = $"Blocked: {reason}. {Notes}";
        SetUpdatedAt();

        AddDomainEvent(new VendorBlockedEvent(Id, Name, reason));
    }

    public bool CanOrder()
    {
        return IsActive && Status == VendorStatus.Active && 
               (CreditLimit == 0 || CurrentBalance <= CreditLimit);
    }

    public decimal AvailableCredit => CreditLimit > 0 ? Math.Max(0, CreditLimit - CurrentBalance) : decimal.MaxValue;

    public string FullAddress => $"{Address}, {City}, {State} {ZipCode}, {Country}";
}

public enum VendorType
{
    Supplier,
    Manufacturer,
    Distributor,
    Wholesaler,
    ServiceProvider
}

public enum VendorStatus
{
    Active,
    Inactive,
    Pending,
    Blocked,
    Suspended
}

public enum PaymentTerms
{
    COD,           // Cash on Delivery
    Net15,         // Payment due in 15 days
    Net30,         // Payment due in 30 days
    Net45,         // Payment due in 45 days
    Net60,         // Payment due in 60 days
    Net90,         // Payment due in 90 days
    PrepaidOnly,   // Payment required before delivery
    TwoTenNet30    // 2% discount if paid within 10 days, otherwise net 30
}