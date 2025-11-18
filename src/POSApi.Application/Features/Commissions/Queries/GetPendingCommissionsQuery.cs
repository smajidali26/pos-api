using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Queries;

public record GetPendingCommissionsQuery : IRequest<List<CommissionTransactionDto>>
{
    public Guid? EmployeeProfileId { get; init; }
}

public class GetPendingCommissionsQueryHandler : IRequestHandler<GetPendingCommissionsQuery, List<CommissionTransactionDto>>
{
    private readonly ICommissionTransactionRepository _repository;

    public GetPendingCommissionsQueryHandler(ICommissionTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CommissionTransactionDto>> Handle(GetPendingCommissionsQuery request, CancellationToken cancellationToken)
    {
        var transactions = await _repository.GetPendingCommissionsAsync(cancellationToken);

        if (request.EmployeeProfileId.HasValue)
            transactions = transactions.Where(t => t.EmployeeProfileId == request.EmployeeProfileId.Value).ToList();

        return transactions.Select(t => new CommissionTransactionDto
        {
            Id = t.Id,
            EmployeeProfileId = t.EmployeeProfileId,
            EmployeeName = t.EmployeeProfile?.User?.FullName ?? "Unknown",
            CommissionId = t.CommissionId,
            CommissionName = t.Commission?.Name ?? "Unknown",
            OrderId = t.OrderId,
            OrderItemId = t.OrderItemId,
            TransactionDate = t.TransactionDate,
            SaleAmount = t.SaleAmount,
            CommissionAmount = t.CommissionAmount,
            Status = t.Status.ToString(),
            PaidDate = t.PaidDate,
            PaymentReference = t.PaymentReference,
            Notes = t.Notes,
            CreatedAt = t.CreatedAt
        }).OrderByDescending(t => t.TransactionDate).ToList();
    }
}

public class CommissionTransactionDto
{
    public Guid Id { get; set; }
    public Guid EmployeeProfileId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public Guid CommissionId { get; set; }
    public string CommissionName { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
    public Guid? OrderItemId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal SaleAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
