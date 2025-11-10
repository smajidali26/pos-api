using Microsoft.Extensions.Logging;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Infrastructure.Services;

public interface IReceiptService
{
    Task<string> GenerateReceiptAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task PrintReceiptAsync(string receiptContent, CancellationToken cancellationToken = default);
}

public class ReceiptService : IReceiptService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(IUnitOfWork unitOfWork, ILogger<ReceiptService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<string> GenerateReceiptAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new InvalidOperationException($"Order with ID {orderId} not found");
        }

        var receipt = $"""
            ================================
                    POS SYSTEM RECEIPT
            ================================
            Order Number: {order.OrderNumber}
            Date: {order.OrderDate:yyyy-MM-dd HH:mm:ss}
            Cashier: {order.Cashier.FullName}
            {(order.Customer != null ? $"Customer: {order.Customer.FullName}" : "")}
            ================================
            
            """;

        foreach (var item in order.OrderItems)
        {
            receipt += $"{item.Product.Name}\n";
            receipt += $"  Qty: {item.Quantity} x ${item.UnitPrice:F2} = ${item.TotalPrice:F2}\n";
            if (item.DiscountAmount > 0)
            {
                receipt += $"  Discount: -${item.DiscountAmount:F2}\n";
            }
            receipt += "\n";
        }

        receipt += $"""
            ================================
            Subtotal: ${order.SubtotalAmount:F2}
            Tax: ${order.TaxAmount:F2}
            {(order.DiscountAmount > 0 ? $"Discount: -${order.DiscountAmount:F2}" : "")}
            Total: ${order.TotalAmount:F2}

            Payment Method: {order.PaymentMethod}
            {(order.CashAmount.HasValue ? $"Cash: ${order.CashAmount.Value:F2}" : "")}
            {(order.CardAmount.HasValue ? $"Card: ${order.CardAmount.Value:F2}" : "")}
            {(order.ChangeAmount.HasValue ? $"Change: ${order.ChangeAmount.Value:F2}" : "")}
            ================================
            Thank you for your business!
            ================================
            """;

        return receipt;
    }

    public async Task PrintReceiptAsync(string receiptContent, CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would send to a thermal printer
        // For now, we'll just log it
        _logger.LogInformation("Receipt printed:\n{Receipt}", receiptContent);
        
        // You could implement actual printer integration here
        await Task.CompletedTask;
    }
}