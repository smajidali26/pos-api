using POSApi.Domain.Common;

namespace POSApi.Domain.Entities;

/// <summary>
/// Defines commission rules for employees/roles
/// </summary>
public class Commission : AggregateRoot
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public CommissionType CommissionType { get; private set; }
    public CommissionBasis CommissionBasis { get; private set; }

    // Rate Configuration
    public decimal Rate { get; private set; } // Percentage or fixed amount
    public decimal? MinimumSaleAmount { get; private set; }
    public decimal? MaximumCommission { get; private set; }

    // Applicability
    public bool IsActive { get; private set; }
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    // Target Configuration
    public Guid? EmployeeProfileId { get; private set; }
    public Guid? ProductId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public string? Role { get; private set; } // Cashier, Manager, etc.

    // Navigation Properties
    public EmployeeProfile? EmployeeProfile { get; set; }
    public Product? Product { get; set; }
    public Category? Category { get; set; }
    public ICollection<CommissionTransaction> Transactions { get; set; } = new List<CommissionTransaction>();

    // Constructor for EF Core
    private Commission() { }

    public Commission(
        string name,
        string description,
        CommissionType commissionType,
        CommissionBasis commissionBasis,
        decimal rate,
        DateTime effectiveFrom,
        decimal? minimumSaleAmount = null,
        decimal? maximumCommission = null,
        Guid? employeeProfileId = null,
        Guid? productId = null,
        Guid? categoryId = null,
        string? role = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Commission name is required", nameof(name));

        if (rate < 0)
            throw new ArgumentException("Commission rate cannot be negative", nameof(rate));

        Name = name;
        Description = description;
        CommissionType = commissionType;
        CommissionBasis = commissionBasis;
        Rate = rate;
        EffectiveFrom = effectiveFrom;
        MinimumSaleAmount = minimumSaleAmount;
        MaximumCommission = maximumCommission;
        EmployeeProfileId = employeeProfileId;
        ProductId = productId;
        CategoryId = categoryId;
        Role = role;
        IsActive = true;
    }

    public void UpdateRate(decimal rate, decimal? minimumSaleAmount = null, decimal? maximumCommission = null)
    {
        if (rate < 0)
            throw new ArgumentException("Commission rate cannot be negative", nameof(rate));

        Rate = rate;
        MinimumSaleAmount = minimumSaleAmount;
        MaximumCommission = maximumCommission;
    }

    public void UpdateEffectiveDates(DateTime effectiveFrom, DateTime? effectiveTo = null)
    {
        if (effectiveTo.HasValue && effectiveTo.Value <= effectiveFrom)
            throw new ArgumentException("Effective to date must be after effective from date");

        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsApplicableToOrder(Order order, Guid employeeProfileId, string? employeeRole)
    {
        // Check if commission is active
        if (!IsActive)
            return false;

        // Check effective dates
        var orderDate = order.OrderDate;
        if (orderDate < EffectiveFrom || (EffectiveTo.HasValue && orderDate > EffectiveTo.Value))
            return false;

        // Check minimum sale amount
        if (MinimumSaleAmount.HasValue && order.TotalAmount < MinimumSaleAmount.Value)
            return false;

        // Check employee-specific commission
        if (EmployeeProfileId.HasValue && EmployeeProfileId.Value != employeeProfileId)
            return false;

        // Check role-specific commission
        if (!string.IsNullOrWhiteSpace(Role) && !string.Equals(Role, employeeRole, StringComparison.OrdinalIgnoreCase))
            return false;

        return true;
    }

    public bool IsApplicableToProduct(Guid productId, Guid? categoryId)
    {
        // Product-specific commission
        if (ProductId.HasValue && ProductId.Value == productId)
            return true;

        // Category-specific commission
        if (CategoryId.HasValue && categoryId.HasValue && CategoryId.Value == categoryId.Value)
            return true;

        // General commission (no specific product/category)
        if (!ProductId.HasValue && !CategoryId.HasValue)
            return true;

        return false;
    }

    public decimal CalculateCommission(decimal saleAmount, int quantity = 1)
    {
        decimal commission = 0;

        switch (CommissionType)
        {
            case CommissionType.Percentage:
                commission = saleAmount * (Rate / 100);
                break;

            case CommissionType.FixedAmount:
                commission = Rate * quantity;
                break;

            case CommissionType.Tiered:
                // Tiered commission can be extended based on sale amount ranges
                commission = saleAmount * (Rate / 100);
                break;
        }

        // Apply maximum commission cap if specified
        if (MaximumCommission.HasValue && commission > MaximumCommission.Value)
            commission = MaximumCommission.Value;

        return Math.Round(commission, 2);
    }
}

public enum CommissionType
{
    Percentage = 0,      // X% of sale amount
    FixedAmount = 1,     // Fixed $ per item/sale
    Tiered = 2           // Different rates based on thresholds
}

public enum CommissionBasis
{
    TotalSale = 0,       // Commission on total order amount
    GrossProfit = 1,     // Commission on (selling price - cost price)
    ItemsSold = 2,       // Commission per item sold
    Category = 3         // Commission on specific categories
}
