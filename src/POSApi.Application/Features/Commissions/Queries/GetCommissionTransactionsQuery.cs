using MediatR;
using POSApi.Domain.Entities;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Queries;

public record GetCommissionTransactionsQuery : IRequest<List<CommissionTransactionDto>>
{
    public Guid? EmployeeProfileId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public CommissionStatus? Status { get; init; }
}

public class GetCommissionTransactionsQueryHandler : IRequestHandler<GetCommissionTransactionsQuery, List<CommissionTransactionDto>>
{
    private readonly ICommissionTransactionRepository _repository;

    public GetCommissionTransactionsQueryHandler(ICommissionTransactionRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<CommissionTransactionDto>> Handle(GetCommissionTransactionsQuery request, CancellationToken cancellationToken)
    {
        List<CommissionTransaction> transactions;

        if (request.EmployeeProfileId.HasValue)
        {
            transactions = await _repository.GetByEmployeeIdAsync(request.EmployeeProfileId.Value, cancellationToken);
        }
        else
        {
            transactions = await _repository.GetAllAsync(cancellationToken);
        }

        // Apply filters
        if (request.StartDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate >= request.StartDate.Value).ToList();

        if (request.EndDate.HasValue)
            transactions = transactions.Where(t => t.TransactionDate <= request.EndDate.Value).ToList();

        if (request.Status.HasValue)
            transactions = transactions.Where(t => t.Status == request.Status.Value).ToList();

        return transactions.Select(t => new CommissionTransactionDto
        {
            Id = t.Id,
            EmployeeProfileId = t.EmployeeProfileId,
            EmployeeName = t.EmployeeProfile?.User?.FullName ?? "Unknown",
            CommissionId = t.CommissionId,
            CommissionName = t.Commission?.Name ?? "Unknown",
            OrderId = t.OrderId,
            OrderNumber = t.Order?.OrderNumber ?? "Unknown",
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
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal SaleAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? PaidDate { get; set; }
    public string? PaymentReference { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}
