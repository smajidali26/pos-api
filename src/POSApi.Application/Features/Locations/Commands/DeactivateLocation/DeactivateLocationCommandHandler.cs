using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Commands.DeactivateLocation;

public class DeactivateLocationCommandHandler : ICommandHandler<DeactivateLocationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateLocationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeactivateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations
            .FindAsync(new object[] { request.LocationId }, cancellationToken);

        if (location == null)
            throw new InvalidOperationException($"Location with ID {request.LocationId} not found");

        location.Deactivate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
