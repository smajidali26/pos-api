namespace POSApi.Application.Common.DTOs.Requests;

public record CreatePaymentIntentRequest
{
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public Guid OrderId { get; init; }
}

public record ProcessCashPaymentRequest
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
}

public record ProcessRefundRequest
{
    public string TransactionId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Reason { get; init; } = string.Empty;
}
