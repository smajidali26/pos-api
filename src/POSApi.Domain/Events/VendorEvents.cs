using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class VendorCreatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string VendorName { get; }
    public string CompanyName { get; }
    public string Email { get; }

    public VendorCreatedEvent(Guid vendorId, string vendorName, string companyName, string email)
    {
        VendorId = vendorId;
        VendorName = vendorName;
        CompanyName = companyName;
        Email = email;
    }
}

public class VendorContactUpdatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string ContactPerson { get; }
    public string Email { get; }
    public string PhoneNumber { get; }

    public VendorContactUpdatedEvent(Guid vendorId, string contactPerson, string email, string phoneNumber)
    {
        VendorId = vendorId;
        ContactPerson = contactPerson;
        Email = email;
        PhoneNumber = phoneNumber;
    }
}

public class VendorPaymentTermsUpdatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public PaymentTerms PaymentTerms { get; }

    public VendorPaymentTermsUpdatedEvent(Guid vendorId, PaymentTerms paymentTerms)
    {
        VendorId = vendorId;
        PaymentTerms = paymentTerms;
    }
}

public class VendorCreditLimitUpdatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public decimal OldLimit { get; }
    public decimal NewLimit { get; }

    public VendorCreditLimitUpdatedEvent(Guid vendorId, decimal oldLimit, decimal newLimit)
    {
        VendorId = vendorId;
        OldLimit = oldLimit;
        NewLimit = newLimit;
    }
}

public class VendorCreditLimitExceededEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string VendorName { get; }
    public decimal CurrentBalance { get; }
    public decimal CreditLimit { get; }

    public VendorCreditLimitExceededEvent(Guid vendorId, string vendorName, decimal currentBalance, decimal creditLimit)
    {
        VendorId = vendorId;
        VendorName = vendorName;
        CurrentBalance = currentBalance;
        CreditLimit = creditLimit;
    }
}

public class VendorBalanceUpdatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public decimal OldBalance { get; }
    public decimal NewBalance { get; }

    public VendorBalanceUpdatedEvent(Guid vendorId, decimal oldBalance, decimal newBalance)
    {
        VendorId = vendorId;
        OldBalance = oldBalance;
        NewBalance = newBalance;
    }
}

public class VendorOrderRecordedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public DateTime OrderDate { get; }
    public decimal OrderAmount { get; }

    public VendorOrderRecordedEvent(Guid vendorId, DateTime orderDate, decimal orderAmount)
    {
        VendorId = vendorId;
        OrderDate = orderDate;
        OrderAmount = orderAmount;
    }
}

public class VendorStatusChangedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public VendorStatus OldStatus { get; }
    public VendorStatus NewStatus { get; }

    public VendorStatusChangedEvent(Guid vendorId, VendorStatus oldStatus, VendorStatus newStatus)
    {
        VendorId = vendorId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}

public class VendorActivatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string VendorName { get; }

    public VendorActivatedEvent(Guid vendorId, string vendorName)
    {
        VendorId = vendorId;
        VendorName = vendorName;
    }
}

public class VendorDeactivatedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string VendorName { get; }

    public VendorDeactivatedEvent(Guid vendorId, string vendorName)
    {
        VendorId = vendorId;
        VendorName = vendorName;
    }
}

public class VendorBlockedEvent : DomainEvent
{
    public Guid VendorId { get; }
    public string VendorName { get; }
    public string Reason { get; }

    public VendorBlockedEvent(Guid vendorId, string vendorName, string reason)
    {
        VendorId = vendorId;
        VendorName = vendorName;
        Reason = reason;
    }
}