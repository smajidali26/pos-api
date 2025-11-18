using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;

namespace POSApi.Application.Features.Inventory.Queries.GetInventoryMovementById;

public class GetInventoryMovementByIdQuery : IQuery<InventoryMovementDto>
{
    public Guid MovementId { get; set; }
}
