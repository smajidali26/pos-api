using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Order : AggregateRoot
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }
    public DateTime OrderDate { get; private set; }
    public decimal SubtotalAmount { get; private set; }  // Renamed from SubTotal for consistency
    public decimal TaxAmount { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }
    public decimal? CashAmount { get; private set; }  // Added for receipt
    public decimal? CardAmount { get; private set; }  // Added for receipt
    public decimal? ChangeAmount { get; private set; }  // Added for receipt
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

    public void Complete(PaymentMethod paymentMethod, string? notes = null, decimal? cashAmount = null, decimal? cardAmount = null, decimal? changeAmount = null)
    {
        PaymentMethod = paymentMethod;
        Notes = notes;
        CashAmount = cashAmount;
        CardAmount = cardAmount;
        ChangeAmount = changeAmount;
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

    public void Refund(string? reason = null)
    {
        if (Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Only completed orders can be refunded");
        }

        Status = OrderStatus.Refunded;
        if (!string.IsNullOrEmpty(reason))
        {
            Notes = string.IsNullOrEmpty(Notes) ? $"Refund: {reason}" : $"{Notes}\nRefund: {reason}";
        }
        SetUpdatedAt();

        AddDomainEvent(new OrderCancelledEvent(Id, OrderNumber, reason ?? "Order refunded"));
    }

    public void PartialRefund(List<(Guid orderItemId, int quantityRefunded)> refundedItems, string? reason = null)
    {
        if (Status != OrderStatus.Completed)
        {
            throw new InvalidOperationException("Only completed orders can be refunded");
        }

        // Process each refunded item
        foreach (var (orderItemId, quantityRefunded) in refundedItems)
        {
            var item = OrderItems.FirstOrDefault(x => x.Id == orderItemId);
            if (item == null)
            {
                throw new InvalidOperationException($"Order item {orderItemId} not found");
            }

            if (quantityRefunded > item.Quantity)
            {
                throw new InvalidOperationException($"Cannot refund more than ordered quantity for item {orderItemId}");
            }

            // Reduce quantity or remove item
            if (quantityRefunded == item.Quantity)
            {
                OrderItems.Remove(item);
            }
            else
            {
                item.UpdateQuantity(item.Quantity - quantityRefunded);
            }
        }

        RecalculateTotal();

        // Check if all items are refunded
        if (!OrderItems.Any() || TotalAmount == 0)
        {
            Status = OrderStatus.Refunded;
        }

        if (!string.IsNullOrEmpty(reason))
        {
            Notes = string.IsNullOrEmpty(Notes) ? $"Partial Refund: {reason}" : $"{Notes}\nPartial Refund: {reason}";
        }

        SetUpdatedAt();
    }

    private void RecalculateTotal()
    {
        SubtotalAmount = OrderItems.Sum(x => x.TotalPrice);
        TaxAmount = 0; // No tax applied
        TotalAmount = SubtotalAmount + TaxAmount - DiscountAmount;
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