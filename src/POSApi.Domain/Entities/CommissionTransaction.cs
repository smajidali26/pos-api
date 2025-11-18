using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Records individual commission earnings for employees
/// </summary>
public class CommissionTransaction : BaseEntity
{
    public Guid EmployeeProfileId { get; private set; }
    public Guid CommissionId { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid? OrderItemId { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public decimal SaleAmount { get; private set; }
    public decimal CommissionAmount { get; private set; }
    public CommissionStatus Status { get; private set; }
    public DateTime? PaidDate { get; private set; }
    public string? PaymentReference { get; private set; }
    public string? Notes { get; private set; }

    // Navigation Properties
    public EmployeeProfile EmployeeProfile { get; set; } = null!;
    public Commission Commission { get; set; } = null!;
    public Order Order { get; set; } = null!;
    public OrderItem? OrderItem { get; set; }

    // Constructor for EF Core
    private CommissionTransaction() { }

    public CommissionTransaction(
        Guid employeeProfileId,
        Guid commissionId,
        Guid orderId,
        DateTime transactionDate,
        decimal saleAmount,
        decimal commissionAmount,
        Guid? orderItemId = null,
        string? notes = null)
    {
        if (saleAmount < 0)
            throw new ArgumentException("Sale amount cannot be negative", nameof(saleAmount));

        if (commissionAmount < 0)
            throw new ArgumentException("Commission amount cannot be negative", nameof(commissionAmount));

        EmployeeProfileId = employeeProfileId;
        CommissionId = commissionId;
        OrderId = orderId;
        OrderItemId = orderItemId;
        TransactionDate = transactionDate;
        SaleAmount = saleAmount;
        CommissionAmount = commissionAmount;
        Status = CommissionStatus.Pending;
        Notes = notes;
    }

    public void Approve()
    {
        if (Status != CommissionStatus.Pending)
            throw new InvalidOperationException($"Cannot approve commission with status {Status}");

        Status = CommissionStatus.Approved;
    }

    public void Pay(DateTime paymentDate, string paymentReference)
    {
        if (Status != CommissionStatus.Approved)
            throw new InvalidOperationException($"Cannot pay commission with status {Status}");

        Status = CommissionStatus.Paid;
        PaidDate = paymentDate;
        PaymentReference = paymentReference;
    }

    public void Void(string reason)
    {
        if (Status == CommissionStatus.Paid)
            throw new InvalidOperationException("Cannot void a paid commission");

        Status = CommissionStatus.Voided;
        Notes = reason;
    }

    public void Adjust(decimal newCommissionAmount, string reason)
    {
        if (Status == CommissionStatus.Paid)
            throw new InvalidOperationException("Cannot adjust a paid commission");

        if (newCommissionAmount < 0)
            throw new ArgumentException("Commission amount cannot be negative", nameof(newCommissionAmount));

        CommissionAmount = newCommissionAmount;
        Notes = reason;
    }
}

public enum CommissionStatus
{
    Pending = 0,
    Approved = 1,
    Paid = 2,
    Voided = 3
}
