namespace POSApi.Application.Common.DTOs.Requests;

public class CreateInterStoreTransferRequest
{
    public Guid FromStoreId { get; set; }
    public Guid ToStoreId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<TransferItemRequest> Items { get; set; } = new();
}

public class TransferItemRequest
{
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public decimal UnitCost { get; set; }
}

public class ApproveTransferRequest
{
    public string? Notes { get; set; }
    public List<ApproveTransferItemRequest>? Items { get; set; }
}

public class ApproveTransferItemRequest
{
    public Guid ProductId { get; set; }
    public int ApprovedQuantity { get; set; }
}

public class RejectTransferRequest
{
    public string Reason { get; set; } = string.Empty;
}

public class ShipTransferRequest
{
    public List<ShipTransferItemRequest>? Items { get; set; }
}

public class ShipTransferItemRequest
{
    public Guid ProductId { get; set; }
    public int ShippedQuantity { get; set; }
}

public class CompleteTransferRequest
{
    public List<CompleteTransferItemRequest>? Items { get; set; }
}

public class CompleteTransferItemRequest
{
    public Guid ProductId { get; set; }
    public int ReceivedQuantity { get; set; }
}

public class CancelTransferRequest
{
    public string Reason { get; set; } = string.Empty;
}
