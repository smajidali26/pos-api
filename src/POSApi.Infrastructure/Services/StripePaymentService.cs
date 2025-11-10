using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace POSApi.Infrastructure.Services;

// Payment Gateway Interfaces and DTOs
public interface IPaymentGatewayService
{
    Task<PaymentGatewayResult> ProcessCardPaymentAsync(string cardToken, decimal amount, string orderId, CancellationToken cancellationToken = default);
    Task<PaymentGatewayResult> ProcessRefundAsync(string transactionId, decimal amount, string reason, CancellationToken cancellationToken = default);
    Task<PaymentGatewayResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, string orderId, CancellationToken cancellationToken = default);
    Task<PaymentIntentResult> GetPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<PaymentIntentResult> CapturePaymentAsync(string paymentIntentId, decimal? amount = null, CancellationToken cancellationToken = default);
    Task<bool> CancelPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<CustomerResult> CreateOrGetCustomerAsync(string email, string? name = null, Dictionary<string, string>? metadata = null, CancellationToken cancellationToken = default);
    Task<bool> AttachPaymentMethodToCustomerAsync(string paymentMethodId, string customerId, CancellationToken cancellationToken = default);
    Task<List<PaymentMethodDetails>> GetCustomerPaymentMethodsAsync(string customerId, CancellationToken cancellationToken = default);
    Task<bool> DetachPaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);
}

public record PaymentGatewayResult
{
    public bool Success { get; init; }
    public string? TransactionId { get; init; }
    public string? AuthorizationCode { get; init; }
    public string? ProcessorResponse { get; init; }
    public string? ErrorMessage { get; init; }
    public decimal Amount { get; init; }
    public string? Status { get; init; }
}

public record PaymentIntentResult
{
    public bool Success { get; init; }
    public string? ClientSecret { get; init; }
    public string? PaymentIntentId { get; init; }
    public string? ErrorMessage { get; init; }
}

public record CustomerResult
{
    public bool Success { get; init; }
    public string? CustomerId { get; init; }
    public string? Email { get; init; }
    public string? Name { get; init; }
    public string? ErrorMessage { get; init; }
}

public record PaymentMethodDetails
{
    public string PaymentMethodId { get; init; } = string.Empty;
    public string? CardLast4 { get; init; }
    public string? CardBrand { get; init; }
    public long? CardExpMonth { get; init; }
    public long? CardExpYear { get; init; }
}

public class StripePaymentService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        IConfiguration configuration,
        ILogger<StripePaymentService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        // Initialize Stripe API key (optional - system can work without Stripe)
        var apiKey = _configuration["Stripe:SecretKey"];
        if (!string.IsNullOrEmpty(apiKey) && !apiKey.Contains("your_") && apiKey.Length > 10)
        {
            StripeConfiguration.ApiKey = apiKey;
            _logger.LogInformation("Stripe payment gateway initialized successfully");
        }
        else
        {
            _logger.LogWarning("Stripe payment gateway not configured. Card payments will not be available. Cash payments will still work.");
        }
    }

    public async Task<PaymentGatewayResult> ProcessCardPaymentAsync(
        string cardToken,
        decimal amount,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        // Check if Stripe is configured
        if (string.IsNullOrEmpty(StripeConfiguration.ApiKey))
        {
            _logger.LogWarning("Stripe payment attempted but gateway not configured");
            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = "Card payment processing is not configured. Please use cash payment or contact administrator."
            };
        }

        try
        {
            // Convert amount to cents (Stripe uses smallest currency unit)
            var amountInCents = (long)(amount * 100);

            // Create payment intent
            var paymentIntentService = new PaymentIntentService();
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "usd", // TODO: Make this configurable
                PaymentMethod = cardToken,
                Confirm = true,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                    AllowRedirects = "never"
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId }
                }
            };

            var paymentIntent = await paymentIntentService.CreateAsync(
                paymentIntentOptions,
                cancellationToken: cancellationToken);

            if (paymentIntent.Status == "succeeded")
            {
                return new PaymentGatewayResult
                {
                    Success = true,
                    TransactionId = paymentIntent.Id,
                    AuthorizationCode = paymentIntent.Id,
                    ProcessorResponse = $"Payment succeeded via Stripe. Status: {paymentIntent.Status}",
                    Amount = amount,
                    Status = paymentIntent.Status
                };
            }
            else if (paymentIntent.Status == "requires_action" || paymentIntent.Status == "requires_payment_method")
            {
                return new PaymentGatewayResult
                {
                    Success = false,
                    TransactionId = paymentIntent.Id,
                    ErrorMessage = $"Payment requires additional action. Status: {paymentIntent.Status}",
                    Status = paymentIntent.Status
                };
            }
            else
            {
                return new PaymentGatewayResult
                {
                    Success = false,
                    TransactionId = paymentIntent.Id,
                    ErrorMessage = $"Payment failed. Status: {paymentIntent.Status}",
                    Status = paymentIntent.Status
                };
            }
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe payment error for order {OrderId}", orderId);

            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message,
                ProcessorResponse = stripeEx.StripeError?.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", orderId);

            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred while processing the payment"
            };
        }
    }

    public async Task<PaymentGatewayResult> ProcessRefundAsync(
        string transactionId,
        decimal amount,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var refundService = new RefundService();
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = transactionId,
                Amount = (long)(amount * 100), // Convert to cents
                Reason = "requested_by_customer",
                Metadata = new Dictionary<string, string>
                {
                    { "refund_reason", reason }
                }
            };

            var refund = await refundService.CreateAsync(refundOptions, cancellationToken: cancellationToken);

            return new PaymentGatewayResult
            {
                Success = refund.Status == "succeeded",
                TransactionId = refund.Id,
                Amount = amount,
                Status = refund.Status,
                ProcessorResponse = $"Refund {refund.Status}"
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Stripe refund error for transaction {TransactionId}", transactionId);

            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing refund for transaction {TransactionId}", transactionId);

            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred while processing the refund"
            };
        }
    }

    public async Task<PaymentGatewayResult> VerifyPaymentAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(transactionId, cancellationToken: cancellationToken);

            return new PaymentGatewayResult
            {
                Success = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                Amount = paymentIntent.Amount / 100m, // Convert from cents
                Status = paymentIntent.Status,
                ProcessorResponse = $"Payment status: {paymentIntent.Status}"
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error verifying payment {TransactionId}", transactionId);

            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message
            };
        }
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        string orderId,
        CancellationToken cancellationToken = default)
    {
        // Check if Stripe is configured
        if (string.IsNullOrEmpty(StripeConfiguration.ApiKey))
        {
            _logger.LogWarning("Payment intent creation attempted but Stripe not configured");
            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = "Card payment processing is not configured. Please use cash payment."
            };
        }

        try
        {
            var paymentIntentService = new PaymentIntentService();
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", orderId }
                }
            };

            var paymentIntent = await paymentIntentService.CreateAsync(options, cancellationToken: cancellationToken);

            return new PaymentIntentResult
            {
                Success = true,
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error creating payment intent for order {OrderId}", orderId);

            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating payment intent for order {OrderId}", orderId);

            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred while creating the payment intent"
            };
        }
    }

    public async Task<PaymentIntentResult> GetPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            return new PaymentIntentResult
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error getting payment intent {PaymentIntentId}", paymentIntentId);

            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message
            };
        }
    }

    public async Task<PaymentIntentResult> CapturePaymentAsync(
        string paymentIntentId,
        decimal? amount = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntentService = new PaymentIntentService();
            var options = new PaymentIntentCaptureOptions();

            if (amount.HasValue)
            {
                options.AmountToCapture = (long)(amount.Value * 100);
            }

            var paymentIntent = await paymentIntentService.CaptureAsync(paymentIntentId, options, cancellationToken: cancellationToken);

            return new PaymentIntentResult
            {
                Success = paymentIntent.Status == "succeeded",
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error capturing payment intent {PaymentIntentId}", paymentIntentId);

            return new PaymentIntentResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message
            };
        }
    }

    public async Task<bool> CancelPaymentIntentAsync(
        string paymentIntentId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = await paymentIntentService.CancelAsync(paymentIntentId, cancellationToken: cancellationToken);

            return paymentIntent.Status == "canceled";
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error canceling payment intent {PaymentIntentId}", paymentIntentId);
            return false;
        }
    }

    public async Task<CustomerResult> CreateOrGetCustomerAsync(
        string email,
        string? name = null,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var customerService = new CustomerService();

            // Try to find existing customer by email
            var searchOptions = new CustomerListOptions
            {
                Email = email,
                Limit = 1
            };

            var customers = await customerService.ListAsync(searchOptions, cancellationToken: cancellationToken);
            var existingCustomer = customers.Data.FirstOrDefault();

            if (existingCustomer != null)
            {
                return new CustomerResult
                {
                    Success = true,
                    CustomerId = existingCustomer.Id,
                    Email = existingCustomer.Email,
                    Name = existingCustomer.Name
                };
            }

            // Create new customer
            var createOptions = new CustomerCreateOptions
            {
                Email = email,
                Name = name,
                Metadata = metadata
            };

            var customer = await customerService.CreateAsync(createOptions, cancellationToken: cancellationToken);

            return new CustomerResult
            {
                Success = true,
                CustomerId = customer.Id,
                Email = customer.Email,
                Name = customer.Name
            };
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error creating/getting customer for email {Email}", email);

            return new CustomerResult
            {
                Success = false,
                ErrorMessage = stripeEx.Message
            };
        }
    }

    public async Task<bool> AttachPaymentMethodToCustomerAsync(
        string paymentMethodId,
        string customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentMethodService = new PaymentMethodService();
            var options = new PaymentMethodAttachOptions
            {
                Customer = customerId
            };

            await paymentMethodService.AttachAsync(paymentMethodId, options, cancellationToken: cancellationToken);
            return true;
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error attaching payment method {PaymentMethodId} to customer {CustomerId}",
                paymentMethodId, customerId);
            return false;
        }
    }

    public async Task<List<PaymentMethodDetails>> GetCustomerPaymentMethodsAsync(
        string customerId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentMethodService = new PaymentMethodService();
            var options = new PaymentMethodListOptions
            {
                Customer = customerId,
                Type = "card"
            };

            var paymentMethods = await paymentMethodService.ListAsync(options, cancellationToken: cancellationToken);

            return paymentMethods.Data.Select(pm => new PaymentMethodDetails
            {
                PaymentMethodId = pm.Id,
                CardLast4 = pm.Card?.Last4,
                CardBrand = pm.Card?.Brand,
                CardExpMonth = pm.Card?.ExpMonth,
                CardExpYear = pm.Card?.ExpYear
            }).ToList();
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error getting payment methods for customer {CustomerId}", customerId);
            return new List<PaymentMethodDetails>();
        }
    }

    public async Task<bool> DetachPaymentMethodAsync(
        string paymentMethodId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var paymentMethodService = new PaymentMethodService();
            await paymentMethodService.DetachAsync(paymentMethodId, cancellationToken: cancellationToken);
            return true;
        }
        catch (StripeException stripeEx)
        {
            _logger.LogError(stripeEx, "Error detaching payment method {PaymentMethodId}", paymentMethodId);
            return false;
        }
    }
}
