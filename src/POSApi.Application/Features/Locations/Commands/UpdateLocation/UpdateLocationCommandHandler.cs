using Microsoft.EntityFrameworkCore;
using POSApi.Application.Common.Interfaces;
using POSApi.Infrastructure.Persistence;

namespace POSApi.Application.Features.Locations.Commands.UpdateLocation;

public class UpdateLocationCommandHandler : ICommandHandler<UpdateLocationCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateLocationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateLocationCommand request, CancellationToken cancellationToken)
    {
        var location = await _unitOfWork.Context.Locations
            .FindAsync(new object[] { request.LocationId }, cancellationToken);

        if (location == null)
            throw new InvalidOperationException($"Location with ID {request.LocationId} not found");

        // Check if code is being changed and if new code already exists
        if (location.Code != request.Code)
        {
            var existingLocation = await _unitOfWork.Context.Locations
                .FirstOrDefaultAsync(l => l.Code == request.Code && l.Id != request.LocationId, cancellationToken);

            if (existingLocation != null)
                throw new InvalidOperationException($"Location with code '{request.Code}' already exists");
        }

        // Validate parent location if specified
        if (request.ParentLocationId.HasValue)
        {
            var parentLocation = await _unitOfWork.Context.Locations
                .FindAsync(new object[] { request.ParentLocationId.Value }, cancellationToken);

            if (parentLocation == null)
                throw new InvalidOperationException($"Parent location with ID {request.ParentLocationId.Value} not found");

            // Prevent circular references
            if (await IsCircularReference(request.LocationId, request.ParentLocationId.Value, cancellationToken))
                throw new InvalidOperationException("Cannot set parent location - would create circular reference");
        }

        location.Update(
            request.Name,
            request.Code,
            request.LocationType,
            request.Description,
            request.ParentLocationId
        );

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<bool> IsCircularReference(Guid locationId, Guid parentId, CancellationToken cancellationToken)
    {
        var currentParentId = parentId;
        while (currentParentId != Guid.Empty)
        {
            if (currentParentId == locationId)
                return true;

            var parent = await _unitOfWork.Context.Locations
                .FindAsync(new object[] { currentParentId }, cancellationToken);

            if (parent?.ParentLocationId == null)
                break;

            currentParentId = parent.ParentLocationId.Value;
        }

        return false;
    }
}
