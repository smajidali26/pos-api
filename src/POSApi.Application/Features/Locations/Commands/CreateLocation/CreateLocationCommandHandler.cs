using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Commands.CreateLocation;

public class CreateLocationCommandHandler : ICommandHandler<CreateLocationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateLocationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateLocationCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var existingLocation = await _unitOfWork.Context.Locations
            .FirstOrDefaultAsync(l => l.Code == request.Code, cancellationToken);

        if (existingLocation != null)
            throw new InvalidOperationException($"Location with code '{request.Code}' already exists");

        // Validate parent location if specified
        if (request.ParentLocationId.HasValue)
        {
            var parentLocation = await _unitOfWork.Context.Locations
                .FindAsync(new object[] { request.ParentLocationId.Value }, cancellationToken);

            if (parentLocation == null)
                throw new InvalidOperationException($"Parent location with ID {request.ParentLocationId.Value} not found");
        }

        var location = new Location(
            request.Name,
            request.Code,
            request.LocationType,
            request.Description,
            request.ParentLocationId
        );

        _unitOfWork.Context.Locations.Add(location);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return location.Id;
    }
}
