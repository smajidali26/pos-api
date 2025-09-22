using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Order : AggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal SubTotal { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public string? Notes { get; private set; }
    public Guid CashierId { get; private set; }
    public User Cashier { get; private set; } = null!;
    public ICollection<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

    private Order() { } // For EF Core

    public Order(string orderNumber, Guid cashierId, Guid? customerId = null)
    {
        OrderNumber = orderNumber;
        CashierId = cashierId;
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;

        AddDomainEvent(new OrderCreatedEvent(Id, orderNumber, cashierId));
    }

    public void AddOrderItem(Guid productId, int quantity, decimal unitPrice, decimal discount = 0)
    {
        var existingItem = OrderItems.FirstOrDefault(x => x.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(Id, productId, quantity, unitPrice, discount);
            OrderItems.Add(orderItem);
        }

        RecalculateTotal();
        SetUpdatedAt();
    }

    public void RemoveOrderItem(Guid productId)
    {
        var item = OrderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            OrderItems.Remove(item);
            RecalculateTotal();
            SetUpdatedAt();
        }
    }

    public void UpdateOrderItemQuantity(Guid productId, int quantity)
    {
        var item = OrderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            if (quantity <= 0)
            {
                RemoveOrderItem(productId);
            }
            else
            {
                item.UpdateQuantity(quantity);
                RecalculateTotal();
                SetUpdatedAt();
            }
        }
    }

    public void ApplyDiscount(decimal discountAmount)
    {
        DiscountAmount = discountAmount;
        RecalculateTotal();
        SetUpdatedAt();
    }

    public void Complete(PaymentMethod paymentMethod, string? notes = null)
    {
        PaymentMethod = paymentMethod;
        Notes = notes;
        Status = OrderStatus.Completed;
        SetUpdatedAt();

        AddDomainEvent(new OrderCompletedEvent(Id, OrderNumber, TotalAmount, paymentMethod));
    }

    public void Cancel(string reason)
    {
        Status = OrderStatus.Cancelled;
        Notes = reason;
        SetUpdatedAt();

        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, reason));
    }

    private void RecalculateTotal()
    {
        SubTotal = OrderItems.Sum(x => x.TotalPrice);
        TaxAmount = SubTotal * 0.08m; // 8% tax rate - this could be configurable
        TotalAmount = SubTotal + TaxAmount - DiscountAmount;
    }
}

public enum OrderStatus
{
    Pending,
    Completed,
    Cancelled,
    Refunded
}

public enum PaymentMethod
{
    Cash,
    CreditCard,
    DebitCard,
    DigitalWallet,
    LoyaltyPoints,
    Check
}