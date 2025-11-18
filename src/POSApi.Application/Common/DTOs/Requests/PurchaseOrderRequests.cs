namespace POSApi.Application.Common.DTOs.Requests;

public class ApprovePurchaseOrderRequest
{
    public Guid ApprovedByUserId { get; set; }
}

public class ReceivePurchaseOrderRequest
{
    public Dictionary<Guid, int> ReceivedQuantities { get; set; } = new();
    public DateTime? ActualDeliveryDate { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CompletePurchaseOrderRequest
{
    public string CompletionNotes { get; set; } = string.Empty;
}

public class CancelPurchaseOrderRequest
{
    public string CancellationReason { get; set; } = string.Empty;
}
