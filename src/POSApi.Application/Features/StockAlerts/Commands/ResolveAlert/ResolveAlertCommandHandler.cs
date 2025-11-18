using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.StockAlerts.Commands.ResolveAlert;

public class ResolveAlertCommandHandler : ICommandHandler<ResolveAlertCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ResolveAlertCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ResolveAlertCommand request, CancellationToken cancellationToken)
    {
        var alert = await _unitOfWork.Context.StockAlerts
            .FindAsync(new object[] { request.AlertId }, cancellationToken);

        if (alert == null)
            throw new InvalidOperationException($"Stock alert with ID {request.AlertId} not found");

        alert.Resolve(request.UserId, request.ResolutionNotes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
