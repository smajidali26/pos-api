using POSApi.Application.Common.DTOs;
using POSApi.Application.Common.Interfaces;
using POSApi.Domain.Entities;

namespace POSApi.Application.Features.Inventory.Queries.GetInventoryMovements;

public class GetInventoryMovementsQuery : IQuery<IEnumerable<InventoryMovementDto>>
{
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public MovementType? MovementType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
