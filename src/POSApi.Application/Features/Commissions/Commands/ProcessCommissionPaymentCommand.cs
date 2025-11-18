using MediatR;
using POSApi.Domain.Repositories;

namespace POSApi.Application.Features.Commissions.Commands;

public record ProcessCommissionPaymentCommand : IRequest<int>
{
    public List<Guid> CommissionTransactionIds { get; init; } = new();
    public DateTime PaymentDate { get; init; }
    public string PaymentReference { get; init; } = string.Empty;
}

public class ProcessCommissionPaymentCommandHandler : IRequestHandler<ProcessCommissionPaymentCommand, int>
{
    private readonly ICommissionTransactionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessCommissionPaymentCommandHandler(
        ICommissionTransactionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(ProcessCommissionPaymentCommand request, CancellationToken cancellationToken)
    {
        int processedCount = 0;

        foreach (var transactionId in request.CommissionTransactionIds)
        {
            var transaction = await _repository.GetByIdAsync(transactionId, cancellationToken);
            if (transaction == null)
                continue;

            try
            {
                // Approve if pending
                if (transaction.Status == Domain.Entities.CommissionStatus.Pending)
                    transaction.Approve();

                // Pay if approved
                if (transaction.Status == Domain.Entities.CommissionStatus.Approved)
                {
                    transaction.Pay(request.PaymentDate, request.PaymentReference);
                    processedCount++;
                }
            }
            catch (InvalidOperationException)
            {
                // Skip transactions that can't be paid
                continue;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return processedCount;
    }
}
