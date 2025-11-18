using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Commands.ActivateLocation;

public class ActivateLocationCommandHandler : ICommandHandler<ActivateLocationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public ActivateLocationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ActivateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations
            .FindAsync(new object[] { request.LocationId }, cancellationToken);

        if (location == null)
            throw new InvalidOperationException($"Location with ID {request.LocationId} not found");

        location.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
