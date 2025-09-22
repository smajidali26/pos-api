using POSApi.Domain.Entities;

namespace POSApi.Application.Common.DTOs;

public class ReturnDto
{
    public Guid Id { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public Guid OriginalOrderId { get; set; }
    public string OriginalOrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public DateTime ReturnDate { get; set; }
    public ReturnReason Reason { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal RefundAmount { get; set; }
    public ReturnStatus Status { get; set; }
    public RefundMethod RefundMethod { get; set; }
    public Guid ProcessedByUserId { get; set; }
    public string ProcessedByName { get; set; } = string.Empty;
    public List<ReturnItemDto> ReturnItems { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Computed properties
    public string OrderNumber { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string ReasonName { get; set; } = string.Empty;
}

public class ReturnItemDto
{
    public Guid Id { get; set; }
    public Guid ReturnId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Reason { get; set; } = string.Empty;
    public decimal TotalRefund { get; set; }
}