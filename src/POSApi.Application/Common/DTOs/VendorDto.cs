using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class VendorDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public VendorType Type { get; set; }
    public VendorStatus Status { get; set; }
    public PaymentTerms PaymentTerms { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit { get; set; }
    public DateTime? LastOrderDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed properties  
    public string StatusName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string PaymentTermsName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int PurchaseOrderCount { get; set; }
}

public class PurchaseOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid VendorId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public PurchaseOrderStatus Status { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PurchaseOrderItemDto
{
    public Guid Id { get; set; }
    public Guid PurchaseOrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public int PendingQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public bool IsFullyReceived { get; set; }
}