using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockAlerts.Commands.AcknowledgeAlert;

public class AcknowledgeAlertCommandHandler : ICommandHandler<AcknowledgeAlertCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public AcknowledgeAlertCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(AcknowledgeAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _unitOfWork.Context.StockAlerts
            .FindAsync(new object[] { request.AlertId }, cancellationToken);

        if (alert == null)
            throw new InvalidOperationException($"Stock alert with ID {request.AlertId} not found");

        alert.Acknowledge(request.UserId, request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
