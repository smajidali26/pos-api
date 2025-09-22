using POSApi.Domain.Common;
using POSApi.Domain.Entities;

namespace POSApi.Domain.Events;

public class PaymentCreatedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public Guid OrderId { get; }
    public decimal Amount { get; }
    public PaymentMethod Method { get; }

    public PaymentCreatedEvent(Guid paymentId, Guid orderId, decimal amount, PaymentMethod method)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Method = method;
    }
}

public class PaymentAuthorizedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public decimal Amount { get; }
    public string AuthorizationCode { get; }

    public PaymentAuthorizedEvent(Guid paymentId, string paymentNumber, decimal amount, string authorizationCode)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        AuthorizationCode = authorizationCode;
    }
}

public class PaymentCompletedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public decimal Amount { get; }
    public PaymentMethod Method { get; }

    public PaymentCompletedEvent(Guid paymentId, string paymentNumber, decimal amount, PaymentMethod method)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        Method = method;
    }
}

public class PaymentFailedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public decimal Amount { get; }
    public string ErrorMessage { get; }

    public PaymentFailedEvent(Guid paymentId, string paymentNumber, decimal amount, string errorMessage)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        ErrorMessage = errorMessage;
    }
}

public class PaymentRefundedEvent : DomainEvent
{
    public Guid PaymentId { get; }
    public string PaymentNumber { get; }
    public decimal RefundAmount { get; }
    public Guid RefundedByUserId { get; }
    public string Reason { get; }

    public PaymentRefundedEvent(Guid paymentId, string paymentNumber, decimal refundAmount, Guid refundedByUserId, string reason)
    {
        PaymentId = paymentId;
        PaymentNumber = paymentNumber;
        RefundAmount = refundAmount;
        RefundedByUserId = refundedByUserId;
        Reason = reason;
    }
}