using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockAlerts.Commands.DismissAlert;

public class DismissAlertCommandHandler : ICommandHandler<DismissAlertCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DismissAlertCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DismissAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _unitOfWork.Context.StockAlerts
            .FindAsync(new object[] { request.AlertId }, cancellationToken);

        if (alert == null)
            throw new InvalidOperationException($"Stock alert with ID {request.AlertId} not found");

        alert.Dismiss(request.DismissReason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
