using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs.Requests;

public class CompleteOrderRequest
{
    public PaymentMethod PaymentMethod { get; set; }
    public string? Notes { get; set; }
}

public class RefundOrderItemsRequest
{
    public List<RefundItemRequest> Items { get; set; } = new();
    public string? Reason { get; set; }
}

public class RefundItemRequest
{
    public Guid OrderItemId { get; set; }
    public int QuantityToRefund { get; set; }
}
