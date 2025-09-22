using POSApi.Domain.Common;
using POSApi.Domain.Events;

namespace POSApi.Domain.Entities;

public class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public Order Order { get; private set; } = null!;
    public string PaymentNumber { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public PaymentStatus Status { get; private set; }
    public DateTime PaymentDate { get; private set; }
    public Guid ProcessedByUserId { get; private set; }
    public User ProcessedBy { get; private set; } = null!;
    public string? TransactionId { get; private set; }
    public string? AuthorizationCode { get; private set; }
    public string? Reference { get; private set; }
    public string? ProcessorResponse { get; private set; }
    public decimal? TipAmount { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    // For split payments
    public ICollection<Payment> SplitPayments { get; private set; } = new List<Payment>();
    public Guid? ParentPaymentId { get; private set; }
    public Payment? ParentPayment { get; private set; }

    private Payment() { } // For EF Core

    public Payment(Guid orderId, string paymentNumber, decimal amount, PaymentMethod method, Guid processedByUserId)
    {
        OrderId = orderId;
        PaymentNumber = paymentNumber;
        Amount = amount;
        Method = method;
        ProcessedByUserId = processedByUserId;
        PaymentDate = DateTime.UtcNow;
        Status = PaymentStatus.Pending;

        AddDomainEvent(new PaymentCreatedEvent(Id, orderId, amount, method));
    }

    public void Authorize(string authorizationCode, string transactionId)
    {
        if (Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException($"Cannot authorize payment in {Status} status");
        }

        Status = PaymentStatus.Authorized;
        AuthorizationCode = authorizationCode;
        TransactionId = transactionId;
        SetUpdatedAt();

        AddDomainEvent(new PaymentAuthorizedEvent(Id, PaymentNumber, Amount, authorizationCode));
    }

    public void Capture(string? processorResponse = null)
    {
        if (Status != PaymentStatus.Authorized)
        {
            throw new InvalidOperationException($"Cannot capture payment in {Status} status");
        }

        Status = PaymentStatus.Completed;
        ProcessorResponse = processorResponse;
        SetUpdatedAt();

        AddDomainEvent(new PaymentCompletedEvent(Id, PaymentNumber, Amount, Method));
    }

    public void Fail(string errorMessage)
    {
        if (Status == PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot fail completed payment");
        }

        Status = PaymentStatus.Failed;
        ProcessorResponse = errorMessage;
        SetUpdatedAt();

        AddDomainEvent(new PaymentFailedEvent(Id, PaymentNumber, Amount, errorMessage));
    }

    public void Refund(decimal refundAmount, Guid refundedByUserId, string reason)
    {
        if (Status != PaymentStatus.Completed)
        {
            throw new InvalidOperationException("Can only refund completed payments");
        }

        if (refundAmount > Amount)
        {
            throw new InvalidOperationException("Refund amount cannot exceed payment amount");
        }

        Status = PaymentStatus.Refunded;
        SetUpdatedAt();

        AddDomainEvent(new PaymentRefundedEvent(Id, PaymentNumber, refundAmount, refundedByUserId, reason));
    }

    public void AddTip(decimal tipAmount)
    {
        if (tipAmount < 0)
        {
            throw new ArgumentException("Tip amount cannot be negative");
        }

        TipAmount = tipAmount;
        SetUpdatedAt();
    }

    public void AddSplitPayment(Payment splitPayment)
    {
        splitPayment.ParentPaymentId = Id;
        SplitPayments.Add(splitPayment);
        SetUpdatedAt();
    }

    public decimal TotalAmount => Amount + (TipAmount ?? 0);
    public bool IsSplitPayment => ParentPaymentId.HasValue;
    public bool HasSplitPayments => SplitPayments.Any();
}

public class PaymentCard : BaseEntity
{
    public string CardNumber { get; private set; } = string.Empty; // Masked
    public string CardholderName { get; private set; } = string.Empty;
    public CardType CardType { get; private set; }
    public string ExpiryMonth { get; private set; } = string.Empty;
    public string ExpiryYear { get; private set; } = string.Empty;
    public string? Token { get; private set; } // For tokenized cards
    public bool IsExpired { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    private PaymentCard() { } // For EF Core

    public PaymentCard(string maskedCardNumber, string cardholderName, CardType cardType, 
                      string expiryMonth, string expiryYear, string? token = null, Guid? customerId = null)
    {
        CardNumber = maskedCardNumber;
        CardholderName = cardholderName;
        CardType = cardType;
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        Token = token;
        CustomerId = customerId;
        IsExpired = CheckIfExpired();
    }

    private bool CheckIfExpired()
    {
        if (!int.TryParse(ExpiryMonth, out var month) || !int.TryParse(ExpiryYear, out var year))
            return true;

        var expiryDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        return DateTime.Now > expiryDate;
    }

    public void UpdateExpiry(string expiryMonth, string expiryYear)
    {
        ExpiryMonth = expiryMonth;
        ExpiryYear = expiryYear;
        IsExpired = CheckIfExpired();
        SetUpdatedAt();
    }
}

public class PaymentProcessor : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public ProcessorType Type { get; private set; }
    public bool IsActive { get; private set; }
    public Dictionary<string, string> Configuration { get; private set; } = new();
    public decimal TransactionFee { get; private set; }
    public decimal TransactionFeePercentage { get; private set; }
    public List<PaymentMethod> SupportedMethods { get; private set; } = new();

    private PaymentProcessor() { } // For EF Core

    public PaymentProcessor(string name, string code, ProcessorType type)
    {
        Name = name;
        Code = code;
        Type = type;
        IsActive = true;
    }

    public void UpdateConfiguration(Dictionary<string, string> configuration)
    {
        Configuration = configuration;
        SetUpdatedAt();
    }

    public void UpdateFees(decimal transactionFee, decimal transactionFeePercentage)
    {
        TransactionFee = transactionFee;
        TransactionFeePercentage = transactionFeePercentage;
        SetUpdatedAt();
    }

    public void AddSupportedMethod(PaymentMethod method)
    {
        if (!SupportedMethods.Contains(method))
        {
            SupportedMethods.Add(method);
            SetUpdatedAt();
        }
    }

    public decimal CalculateFee(decimal amount)
    {
        return TransactionFee + (amount * TransactionFeePercentage / 100);
    }
}

public enum PaymentStatus
{
    Pending,
    Authorized,
    Completed,
    Failed,
    Cancelled,
    Refunded,
    PartiallyRefunded
}

public enum CardType
{
    Visa,
    MasterCard,
    AmericanExpress,
    Discover,
    JCB,
    DinersClub,
    UnionPay,
    Unknown
}

public enum ProcessorType
{
    CreditCard,
    DebitCard,
    DigitalWallet,
    BankTransfer,
    Cryptocurrency,
    GiftCard
}